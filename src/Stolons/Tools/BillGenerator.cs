using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using System.IO;
using Stolons.Models;
using System.Threading;
using System.Globalization;
using OfficeOpenXml.Style;
using Stolons.Helpers;
using Stolons.Services;
using Microsoft.EntityFrameworkCore;

namespace Stolons.Tools
{
    public static  class BillGenerator
    {
        public class BillEntryConsumer
        {
            public BillEntry BillEntry { get; set; }
            public Consumer Consumer { get; set; }

            public BillEntryConsumer(BillEntry billEntry, Consumer consumer)
            {
                BillEntry = billEntry;
                Consumer = consumer;
            }
        }

        public static void ManageBills(ApplicationDbContext dbContext)
        {
            ApplicationConfig.Modes lastMode = Configurations.Mode;
            do
            {
                ApplicationConfig.Modes currentMode = Configurations.Mode;
                if (lastMode == ApplicationConfig.Modes.Order && currentMode == ApplicationConfig.Modes.DeliveryAndStockUpdate)
                {
                    //We moved form Order to Preparation, create and send bills
                    List<IBill> consumerBills = new List<IBill>();
                    List<IBill> producerBills = new List<IBill>();
                    Dictionary<Producer, List<BillEntryConsumer>> brutProducerBills = new Dictionary<Producer, List<BillEntryConsumer>>();
                    //Consumer (create bills)
                    List<ValidatedWeekBasket> consumerWeekBaskets = dbContext.ValidatedWeekBaskets.Include(x => x.Products).Include(x => x.Consumer).ToList();
                    GenerateBill(consumerWeekBaskets,dbContext);
                    foreach (var weekBasket in consumerWeekBaskets)
                    {
                        //Generate bill for consumer
                        ConsumerBill consumerBill = GenerateBill(weekBasket, dbContext);
                        consumerBills.Add(consumerBill);
                        dbContext.Add(consumerBill);
                        //Add to producer bill entry
                        foreach (var tmpBillEntry in weekBasket.Products)
                        {
                            var billEntry = dbContext.BillEntrys.Include(x => x.Product).ThenInclude(x => x.Producer).First(x=>x.Id == tmpBillEntry.Id);
                            Producer producer = billEntry.Product.Producer;
                            if (!brutProducerBills.ContainsKey(producer))
                            {
                                brutProducerBills.Add(producer, new List<BillEntryConsumer>());
                            }
                            brutProducerBills[producer].Add(new BillEntryConsumer(billEntry,weekBasket.Consumer));
                        }
                    }
                    //Producer (creates bills)
                    foreach (var producerBill in brutProducerBills)
                    {
                        //Generate bill for producer
                        ProducerBill bill = GenerateBill(producerBill.Key, producerBill.Value, dbContext);
                        producerBills.Add(bill);
                        dbContext.Add(bill);
                        //Send mail to producer
                        AuthMessageSender.SendEmail(bill.Producer.Email, 
                                                        bill.Producer.CompanyName,
                                                        "Votre commande de la semaine (Facture "+ bill.BillNumber +")",
                                                        "<h3>En pièce jointe votre commande de la semaine (Facture " + bill.BillNumber + ")</h3>", 
                                                        File.ReadAllBytes(bill.GetFilePath()),
                                                        "Facture "+ bill.BillNumber + ".xlsx");
                    }
                    // => Producer, send mails
                    foreach (var producer in dbContext.Producers.Where(x=> !brutProducerBills.Keys.Contains(x)))
                    {
                        //Un mail à tout les producteurs n'ayant pas de commande
                        AuthMessageSender.SendEmail(producer.Email, producer.CompanyName, "Aucune commande cette semaine", "<h3>Vous n'avez pas de commande cette semaine</h3>");
                    }
                    //Bills (save bills and send mails to user)
                    foreach(var bill in consumerBills)
                    {
                        dbContext.Add(bill);
                        //Send mail to user with bill
                        string message = "<h3>"+Configurations.ApplicationConfig.OrderDeliveryMessage+"</h3>";
                        message += "<br/>";
                        message += "<h4>En pièce jointe votre commande de la semaine (Facture " + bill.BillNumber + ")</h4>";

                        AuthMessageSender.SendEmail(bill.User.Email,
                                                        bill.User.Name,
                                                        "Votre commande de la semaine (Facture " + bill.BillNumber + ")", 
                                                        message,
                                                        File.ReadAllBytes(bill.GetFilePath()),
                                                        "Facture " + bill.BillNumber + ".xlsx");
                    }
                    //Remove week basket
                    dbContext.TempsWeekBaskets.Clear();
                    dbContext.ValidatedWeekBaskets.Clear();
                    dbContext.BillEntrys.Clear();
                    //Move product to, to validate
                    dbContext.Products.ToList().ForEach(x => x.State = Product.ProductState.Stock);

                    #if (DEBUG)
                        //For test, remove existing consumer bill and producer bill => That will never exit in normal mode cause they can only have one bill by week per user
                        dbContext.RemoveRange(dbContext.ConsumerBills.Where(x=> consumerBills.Any(y=>y.BillNumber == x.BillNumber)));
                        dbContext.RemoveRange(dbContext.ProducerBills.Where(x => producerBills.Any(y => y.BillNumber == x.BillNumber)));
                    #endif
                    //
                    dbContext.SaveChanges();
                    //Set product remaining stock to week stock value
                    dbContext.Products.ToList().ForEach(x => x.RemainingStock = x.WeekStock);
                    dbContext.SaveChanges();
                }
                if(lastMode == ApplicationConfig.Modes.DeliveryAndStockUpdate && currentMode == ApplicationConfig.Modes.Order)
                {
                    foreach( var product in dbContext.Products.Where(x => x.State == Product.ProductState.Stock))
                    {
                        product.State = Product.ProductState.Disabled;
                    }
                    dbContext.SaveChanges();
                }
                lastMode = currentMode;
                Thread.Sleep(5000);
            } while (true);
        }

        private static string GetFilePath(this IBill bill)
        {
            return Path.Combine(Configurations.Environment.WebRootPath,
                                            bill.User is Producer ? Configurations.ProducersBillsStockagePath : Configurations.ConsumersBillsStockagePath,
                                            bill.User.Id.ToString(),
                                            bill.BillNumber + ".xlsx");
        }

        private static void GenerateBill(List<ValidatedWeekBasket> consumerWeekBaskets, ApplicationDbContext dbContext)
        {
            //Generate exel file with bill number for user
	        #region File creation
	        string billNumber = DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear();
            string consumerBillsPath = Path.Combine(Configurations.Environment.WebRootPath, Configurations.StolonsBillsStockagePath);
	        string newBillPath = Path.Combine(consumerBillsPath, billNumber + ".xlsx");
	        FileInfo newFile = new FileInfo(newBillPath);
	        if (newFile.Exists)
            {
                //Normaly impossible
                newFile.Delete();  // ensures we create a new workbook
		        newFile = new FileInfo(newBillPath);
            }
            else
            {
                Directory.CreateDirectory(consumerBillsPath);
            }
            #endregion File creation
            //
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                if(!consumerWeekBaskets.Any())
                {
                    //Rien de commander cette semaine :'(
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Rien :'(");
                    worksheet.Cells[1, 1].Value = "Rien cette semaine !";
                }
                else
                {
                    foreach (ValidatedWeekBasket weekBasket in consumerWeekBaskets.OrderBy(x => x.Consumer.Id))
                    {
                        // add a new worksheet to the empty workbook
                        ExcelWorksheet worksheet = package.Workbook.Worksheets.Add(weekBasket.Consumer.Id.ToString() + " (" + weekBasket.Consumer.Name + ")");
                        int row = 1;
                        //Add global informations
                        worksheet.Cells[row, 1, 8, 3].Merge = true;
                        worksheet.Cells[row, 1, 8, 3].Value = weekBasket.Consumer.Id;
                        worksheet.Cells[row, 1, 8, 3].Style.Font.Size = 100;
                        worksheet.Cells[row, 1, 8, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 1, 8, 3].Style.VerticalAlignment = ExcelVerticalAlignment.Center;

                        worksheet.Cells[row, 4].Value = "Facture";
                        worksheet.Cells[row, 5].Value = billNumber;
                        row++;
                        worksheet.Cells[row, 4].Value = "Semaine";
                        worksheet.Cells[row, 5].Value = DateTime.Now.GetIso8601WeekOfYear();
                        row++;
                        worksheet.Cells[row, 4].Value = "Nom";
                        worksheet.Cells[row, 5].Value = weekBasket.Consumer.Name;
                        row++;
                        worksheet.Cells[row, 4].Value = "Prénom";
                        worksheet.Cells[row, 5].Value = weekBasket.Consumer.Surname;
                        row++;
                        worksheet.Cells[row, 4].Value = "Téléphone";
                        worksheet.Cells[row, 5].Value = weekBasket.Consumer.PhoneNumber;
                        worksheet.Cells[1, 4, row, 4].Style.Font.Bold = true;
                        worksheet.Cells[1, 5, row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        row++;
                        worksheet.Cells[row, 4, row + 1, 5].Merge = true;
                        var total = worksheet.Cells[row, 6, row + 1, 6];
                        total.Merge = true;
                        worksheet.Cells[row, 4, row + 1, 5].Value = "TOTAL";
                        worksheet.Cells[row, 4, row + 1, 6].Style.Font.Size = 18;
                        worksheet.Cells[row, 4, row + 1, 6].Style.Font.Bold = true;
                        worksheet.Cells[row, 4, row + 1, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        worksheet.Cells[row, 4, row + 1, 6].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                        //Add product informations
                        row++;
                        row++;
                        row++;
                        worksheet.Cells[row, 1, row, 2].Merge = true;
                        worksheet.Cells[row, 1, row, 2].Value = "NOM";
                        worksheet.Cells[row, 3].Value = "TYPE";
                        worksheet.Cells[row, 4].Value = "PRIX UNITAIRE";
                        worksheet.Cells[row, 5].Value = "QUANTITE";
                        worksheet.Cells[row, 6].Value = "PRIX TOTAL";

                        //Create list of bill entry by product
                        Dictionary<Producer, List<BillEntry>> producersProducts = new Dictionary<Producer, List<BillEntry>>();
                        foreach (var billEntryConsumer in weekBasket.Products)
                        {
                            var billEntry = dbContext.BillEntrys.Include(x => x.Product).ThenInclude(x=>x.Producer).First(x => x.Id == billEntryConsumer.Id);
                            if (!producersProducts.ContainsKey(billEntry.Product.Producer))
                            {
                                producersProducts.Add(billEntry.Product.Producer, new List<BillEntry>());
                            }
                            producersProducts[billEntry.Product.Producer].Add(billEntry);
                        }
                        List<int> rowsTotal = new List<int>();
                        // - Add products by producer
                        foreach (var producer in producersProducts.Keys.OrderBy(x => x.Id))
                        {
                            row++;
                            worksheet.Cells[row, 1, row + 1, 6].Merge = true;
                            worksheet.Cells[row, 1, row + 1, 6].Value = producer.CompanyName;
                            worksheet.Cells[row, 1, row + 1, 6].Style.Font.Size = 22;
                            worksheet.Cells[row, 1, row + 1, 6].Style.Font.Bold = true;
                            row++;
                            row++;
                            int startRow = row;
                            foreach (var billEntry in producersProducts[producer].OrderBy(x => x.Product.Name))
                            {
                                worksheet.Cells[row, 1, row, 2].Merge = true;
                                worksheet.Cells[row, 1, row, 2].Value = billEntry.Product.Name; ;
                                string typeDetail = billEntry.Product.Type == Product.SellType.Piece ? "" : " par " + billEntry.Product.QuantityStepString;
                                worksheet.Cells[row, 3].Value = EnumHelper<Product.SellType>.GetDisplayValue(billEntry.Product.Type) + typeDetail;
                                worksheet.Cells[row, 4].Value = billEntry.Product.UnitPrice;
                                worksheet.Cells[row, 4].Style.Numberformat.Format = "0.00€";
                                worksheet.Cells[row, 5].Value = billEntry.QuantityString;
                                worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                                worksheet.Cells[row, 6].Formula = billEntry.Quantity + "*" + new ExcelCellAddress(row, 4).Address;
                                worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00€";
                                worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;
                                worksheet.Cells[row, 1, row, 6].Style.Font.Size = 13;
                                worksheet.Cells[row, 1, row, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                                row++;
                            }
                            //Total
                            worksheet.Cells[row, 5].Value = "TOTAL : ";
                            worksheet.Cells[row, 6].Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(startRow, 6, row - 1, 6).Address);
                            worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00€";
                            worksheet.Cells[row, 5, row, 6].Style.Font.Bold = true;
                            worksheet.Cells[row, 5, row, 6].Style.Font.Size = 13;
                            worksheet.Cells[row, 5, row, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                            rowsTotal.Add(row);
                        }

                        //Super total
                        string totalFormula = "";
                        for (int cpt = 0; cpt < rowsTotal.Count; cpt++)
                        {
                            totalFormula += new ExcelCellAddress(rowsTotal[cpt], 6).Address;
                            if (cpt != rowsTotal.Count - 1)
                            {
                                totalFormula += "+";
                            }
                        }
                        total.Formula = totalFormula;
                        total.Style.Numberformat.Format = "0.00€";
                        //
                        worksheet.View.PageLayoutView = true;
                        worksheet.Column(1).Width = (98 - 12 + 5) / 7d + 1;
                        worksheet.Column(2).Width = (98 - 12 + 5) / 7d + 1;
                        worksheet.Column(3).Width = (134 - 12 + 5) / 7d + 1;
                        worksheet.Column(4).Width = (98 - 12 + 5) / 7d + 1;
                        worksheet.Column(5).Width = (80 - 12 + 5) / 7d + 1;
                        worksheet.Column(6).Width = (80 - 12 + 5) / 7d + 1;
                    }
                }
             


                // Document properties
                package.Workbook.Properties.Title = "Factures : " + billNumber;
                package.Workbook.Properties.Author = "Stolons";
                package.Workbook.Properties.Comments = "Factures des adhérants de la semaine " + billNumber;

                // Extended property values
                package.Workbook.Properties.Company = "Association Stolons";
                // save our new workbook and we are done!
                package.Save();
            }
        }

        /*
        *BILL NAME INFORMATION
        *Bills are stored like that : bills\UserId\Year_WeekNumber_UserId
        */


        private static ProducerBill GenerateBill(Producer producer, List<BillEntryConsumer> billEntries, ApplicationDbContext dbContext)
        {
            ProducerBill bill = CreateBill<ProducerBill>(producer);
            //Generate exel file with bill number for user
            string producerBillsPath = Path.Combine(Configurations.Environment.WebRootPath, Configurations.ProducersBillsStockagePath, bill.User.Id.ToString());
	        string newBillPath = Path.Combine(producerBillsPath, bill.BillNumber + ".xlsx");
            FileInfo newFile = new FileInfo(newBillPath);
            if (newFile.Exists)
            {
                //Normaly impossible
                newFile.Delete();  // ensures we create a new workbook
		        newFile = new FileInfo(newBillPath);
            }
            else
            {
                Directory.CreateDirectory(producerBillsPath);
            }
            
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                #region Facture par produit
                // add a new worksheet to the empty workbook
                ExcelWorksheet worksheetByProduct = package.Workbook.Worksheets.Add("Facture par produit");
                int row = 1;
                //Add global informations
                worksheetByProduct.Cells[row, 1].Value = "Producteur :";
                worksheetByProduct.Cells[row, 2].Value = producer.CompanyName;
                worksheetByProduct.Cells[row, 2].Style.Font.Bold = true;
                worksheetByProduct.Cells[row ,2].Style.Font.Size = 14;
                row++;
                worksheetByProduct.Cells[row, 1].Value = "Numéro de facture :";
                worksheetByProduct.Cells[row, 2].Value = bill.BillNumber;
                row++;
                worksheetByProduct.Cells[row, 1].Value = "Année :";
                worksheetByProduct.Cells[row, 2].Value = DateTime.Now.Year;
                row++;
                worksheetByProduct.Cells[row, 1].Value = "Semaine :";
                worksheetByProduct.Cells[row, 2].Value = DateTime.Now.GetIso8601WeekOfYear();
                row++;
                worksheetByProduct.Cells[1, 1, row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheetByProduct.Cells[1, 1, row, 1].Style.Font.Bold = true;
                //Add product informations
                row++;
                row++;
                //Create list of bill entry by product
                Dictionary<Product, List<BillEntryConsumer>> products = new Dictionary<Product, List<BillEntryConsumer>>();
                foreach (var billEntryConsumer in billEntries)
                {
                    if(!products.ContainsKey(billEntryConsumer.BillEntry.Product))
                    {
                        products.Add(billEntryConsumer.BillEntry.Product, new List<BillEntryConsumer>());
                    }
                    products[billEntryConsumer.BillEntry.Product].Add(billEntryConsumer);
                }
                List<int> rowsTotal = new List<int>();
                // - Add products
                foreach (var prod in products)
                {
                    // - Add the headers
                    string typeDetail = prod.Key.Type == Product.SellType.Piece ? "" : " par " + prod.Key.QuantityStepString;
                    worksheetByProduct.Cells[row, 2].Value = EnumHelper<Product.SellType>.GetDisplayValue(prod.Key.Type) + typeDetail;
                    worksheetByProduct.Cells[row, 4].Value = prod.Key.UnitPrice;
                    worksheetByProduct.Cells[row, 4].Style.Numberformat.Format = "0.00€";
                    worksheetByProduct.Cells[row, 2, row, 4].Style.Font.Bold = true;
                    worksheetByProduct.Cells[row, 2, row, 4].Style.Font.Size = 12;
                    worksheetByProduct.Cells[row, 2, row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheetByProduct.Cells[row, 2, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    int unitPriceRow = row;
                    row++;
                    worksheetByProduct.Cells[row - 1, 1, row, 1].Merge = true;
                    worksheetByProduct.Cells[row - 1, 1, row, 1].Value = prod.Key.Name;
                    worksheetByProduct.Cells[row - 1, 1, row, 1].Style.Font.Bold = true;
                    worksheetByProduct.Cells[row - 1, 1, row, 1].Style.Font.Size = 16;
                    worksheetByProduct.Cells[row - 1, 1, row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheetByProduct.Cells[row - 1, 1, row, 1].Style.VerticalAlignment = ExcelVerticalAlignment.Center;
                    worksheetByProduct.Cells[row - 1, 1, row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);

                    worksheetByProduct.Cells[row, 2].Value = "Gestion";
                    worksheetByProduct.Cells[row, 3].Value = "Quantité";
                    worksheetByProduct.Cells[row, 4].Value = "Prix total";
                    worksheetByProduct.Cells[row, 2, row, 4].Style.Font.Bold = true;
                    worksheetByProduct.Cells[row, 2, row, 4].Style.Font.Size = 12;
                    worksheetByProduct.Cells[row, 2, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    row++;
                    int productStartRow = row;
                    foreach (var billEntryConsumer in prod.Value.OrderBy(x => x.Consumer.Id))
                    {
                        worksheetByProduct.Cells[row, 1].Value = "• " + billEntryConsumer.Consumer.Id;
                        worksheetByProduct.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheetByProduct.Cells[row, 2].Value = billEntryConsumer.BillEntry.Quantity;
                        worksheetByProduct.Cells[row, 3].Value = billEntryConsumer.BillEntry.QuantityString;
                        worksheetByProduct.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheetByProduct.Cells[row, 4].Formula = new ExcelCellAddress(row, 2).Address + "*" + new ExcelCellAddress(unitPriceRow, 4).Address;
                        worksheetByProduct.Cells[row, 4].Style.Numberformat.Format = "0.00€";
                        worksheetByProduct.Cells[row, 5].Value = "☐";
                        worksheetByProduct.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

                        row++;
                    }
                    worksheetByProduct.Cells[productStartRow, 1, row, 1].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheetByProduct.Cells[productStartRow, 2, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheetByProduct.Cells[productStartRow, 2, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    rowsTotal.Add(row);
                    //TOTAL SANS COMISSION
                    worksheetByProduct.Cells[row, 1].Value = "Total sans comission";
                    worksheetByProduct.Cells[row, 2].Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(productStartRow, 2, row - 1, 2).Address);
                    worksheetByProduct.Cells[row, 3].Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(productStartRow, 2, row - 1, 2).Address);
                    worksheetByProduct.Cells[row, 4].Formula = new ExcelCellAddress(row, 2).Address + "*" + new ExcelCellAddress(unitPriceRow, 4).Address;
                    worksheetByProduct.Cells[row, 4].Style.Numberformat.Format = "0.00€";
                    worksheetByProduct.Cells[row, 1, row, 4].Style.Font.Size = 9;
                    worksheetByProduct.Cells[row, 1, row, 4].Style.Font.Italic = true;
                    worksheetByProduct.Cells[row, 1, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheetByProduct.Cells[row, 1, row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    row++;
                    //COMISSION
                    worksheetByProduct.Cells[row, 1].Value = "Comission";
                    worksheetByProduct.Cells[row, 2].Value = Configurations.ApplicationConfig.Comission + "%";
                    worksheetByProduct.Cells[row, 3].Value = Configurations.ApplicationConfig.Comission + "%";
                    worksheetByProduct.Cells[row, 4].Formula = new ExcelCellAddress(row, 2).Address + "*" + new ExcelCellAddress(row - 1, 4).Address;
                    worksheetByProduct.Cells[row, 4].Style.Numberformat.Format = "0.00€";
                    worksheetByProduct.Cells[row, 1, row, 4].Style.Font.Size = 9;
                    worksheetByProduct.Cells[row, 1, row, 4].Style.Font.Italic = true;
                    worksheetByProduct.Cells[row, 1, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheetByProduct.Cells[row, 1, row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    row++;
                    //TOTAL AVEC COMISSION
                    worksheetByProduct.Cells[row, 1].Value = "TOTAL";
                    worksheetByProduct.Cells[row, 1].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheetByProduct.Cells[row, 2].Formula = new ExcelCellAddress(row -2, 2).Address;
                    worksheetByProduct.Cells[row, 2].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    int totalQuantity = 0;
                    prod.Value.ForEach(billEntry => totalQuantity += billEntry.BillEntry.Quantity);
                    worksheetByProduct.Cells[row, 3].Value = prod.Key.GetQuantityString(totalQuantity);
                    worksheetByProduct.Cells[row, 4].Formula = new ExcelCellAddress(row - 2, 4).Address + "-" + new ExcelCellAddress(row - 1, 4).Address;
                    worksheetByProduct.Cells[row, 4].Style.Numberformat.Format = "0.00€";
                    worksheetByProduct.Cells[row, 1, row, 4].Style.Font.Bold = true;
                    worksheetByProduct.Cells[row, 1, row, 4].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheetByProduct.Cells[row, 5].Value = "☐";
                    worksheetByProduct.Cells[row, 1, row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheetByProduct.Cells[row, 1, row, 5].Style.Font.Size = 14;
                    //
                    //Next product
                    row++;
                    row++;
                }
                //Super total
                string totalWhitoutComissionFormula ="";
                string totalComission = "";
                for (int cpt = 0; cpt<rowsTotal.Count;cpt++)
                {
                    totalWhitoutComissionFormula += new ExcelCellAddress(rowsTotal[cpt], 4).Address;
                    totalComission += new ExcelCellAddress(rowsTotal[cpt] +1, 4).Address;
                    if (cpt != rowsTotal.Count -1)
                    {
                        totalWhitoutComissionFormula += "+";
                        totalComission += "+";
                    }
                }
                worksheetByProduct.Cells[row, 3].Value = "Total sans comission";
                worksheetByProduct.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheetByProduct.Cells[row, 4].Formula = totalWhitoutComissionFormula;
                worksheetByProduct.Cells[row, 4].Style.Numberformat.Format = "0.00€";
                worksheetByProduct.Cells[row, 3, row, 4].Style.Font.Italic = true;
                worksheetByProduct.Cells[row, 3, row, 4].Style.Font.Size = 9;
                row++;
                worksheetByProduct.Cells[row, 3].Value = "Total comission à " + Configurations.ApplicationConfig.Comission + "%";
                worksheetByProduct.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheetByProduct.Cells[row, 4].Formula = totalComission;
                worksheetByProduct.Cells[row, 4].Style.Numberformat.Format = "0.00€";
                worksheetByProduct.Cells[row, 3, row, 4].Style.Font.Italic = true;
                worksheetByProduct.Cells[row, 3, row, 4].Style.Font.Size = 9;
                row++;
                worksheetByProduct.Cells[row, 3].Value = "TOTAL :";
                worksheetByProduct.Cells[row, 3].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheetByProduct.Cells[row, 4].Formula = new ExcelCellAddress(row - 2, 4).Address + "-" + new ExcelCellAddress(row - 1, 4).Address;
                worksheetByProduct.Cells[row, 4].Style.Numberformat.Format = "0.00€";
                worksheetByProduct.Cells[row, 3, row, 4].Style.Font.Bold = true;
                worksheetByProduct.Cells[row, 3, row ,4].Style.Font.Size = 18;
                //
                worksheetByProduct.View.PageLayoutView = true;
                worksheetByProduct.Column(1).Width = (290 - 12 + 5) / 7d + 1;
                worksheetByProduct.Column(2).Width = 0;
                worksheetByProduct.Column(3).Width = (130 - 12 + 5) / 7d + 1;
                worksheetByProduct.Column(4).Width = (130 - 12 + 5) / 7d + 1;
                worksheetByProduct.Column(5).Width = (40 - 12 + 5) / 7d + 1;
                #endregion Facture par produit

                #region Facture par client
                // add a new worksheet to the empty workbook
                ExcelWorksheet worksheetByClient = package.Workbook.Worksheets.Add("Facture par client");
                row = 1;
                //Add global informations
                worksheetByClient.Cells[row, 1].Value = "Producteur :";
                worksheetByClient.Cells[row, 2].Value = producer.CompanyName;
                worksheetByClient.Cells[row, 2].Style.Font.Bold = true;
                worksheetByClient.Cells[row, 2].Style.Font.Size = 14;
                row++;
                worksheetByClient.Cells[row, 1].Value = "Numéro de facture :";
                worksheetByClient.Cells[row, 2].Value = bill.BillNumber;
                row++;
                worksheetByClient.Cells[row, 1].Value = "Année :";
                worksheetByClient.Cells[row, 2].Value = DateTime.Now.Year;
                row++;
                worksheetByClient.Cells[row, 1].Value = "Semaine :";
                worksheetByClient.Cells[row, 2].Value = DateTime.Now.GetIso8601WeekOfYear();
                row++;
                worksheetByClient.Cells[1, 1, row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Left;
                worksheetByClient.Cells[1, 1, row, 1].Style.Font.Bold = true;
                //Add product informations
                row++;
                row++;
                rowsTotal = new List<int>();
                var billEntriesByConsumer = billEntries.GroupBy(x => x.Consumer);
                foreach (var group in billEntriesByConsumer.OrderBy(x=>x.Key.Id))
                {
                    var clientId = worksheetByClient.Cells[row, 1, row, 5];
                    clientId.Merge = true;
                    clientId.Value = group.Key.Id;
                    clientId.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    clientId.Style.Font.Size = 26;
                    clientId.Style.Font.Bold = true;
                    clientId.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                    row++;
                    int productStartRow = row;
                    foreach (var entries in group.OrderBy(x=>x.BillEntry.Product.Name))
                    {
                        // 1 Nom > 2 Type > 3 Prix Unitaire > 4 Quantité Total >  5 Prix Total
                        worksheetByClient.Cells[row, 1].Value = entries.BillEntry.Product.Name;
                        worksheetByClient.Cells[row, 1].Style.Font.Bold = true;
                        string typeDetail = entries.BillEntry.Product.Type == Product.SellType.Piece ? "" : " par " + entries.BillEntry.Product.QuantityStepString;
                        worksheetByClient.Cells[row, 2].Value = EnumHelper<Product.SellType>.GetDisplayValue(entries.BillEntry.Product.Type) + typeDetail;
                        worksheetByClient.Cells[row, 3].Value = entries.BillEntry.Product.UnitPrice;
                        worksheetByClient.Cells[row, 3].Style.Numberformat.Format = "0.00€";
                        worksheetByClient.Cells[row, 4].Value = entries.BillEntry.QuantityString;
                        worksheetByClient.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                        worksheetByClient.Cells[row, 5].Formula = entries.BillEntry.Quantity + "*" + worksheetByClient.Cells[row, 3].Address;
                        worksheetByClient.Cells[row, 5].Style.Numberformat.Format = "0.00€";
                        var line = worksheetByClient.Cells[row, 1, row, 5];
                        line.Style.Font.Size = 13;
                        line.Style.Border.BorderAround(ExcelBorderStyle.Medium);
                        worksheetByClient.Cells[row, 6].Value = "☐";
                        worksheetByClient.Cells[row, 6].Style.Font.Size = 10;
                        worksheetByClient.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                        row++;
                    }
                    rowsTotal.Add(row);
                    //TOTAL SANS COMISSION
                    worksheetByClient.Cells[row, 4].Value = "Sans comission";
                    worksheetByClient.Cells[row, 5].Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(productStartRow, 5, row - 1, 5).Address);
                    worksheetByClient.Cells[row, 5].Style.Numberformat.Format = "0.00€";
                    worksheetByClient.Cells[row, 4, row, 5].Style.Font.Size = 9;
                    worksheetByClient.Cells[row, 4, row, 5].Style.Font.Italic = true;
                    worksheetByClient.Cells[row, 4, row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheetByClient.Cells[row, 4, row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    row++;
                    //COMISSION
                    worksheetByClient.Cells[row, 4].Value = "Comission à "+ Configurations.ApplicationConfig.Comission + " %";
                    worksheetByClient.Cells[row, 5].Formula = (Configurations.ApplicationConfig.Comission + "%") + " * " + new ExcelCellAddress(row - 1, 5).Address;
                    worksheetByClient.Cells[row, 5].Style.Numberformat.Format = "0.00€";
                    worksheetByClient.Cells[row, 4, row, 5].Style.Font.Size = 9;
                    worksheetByClient.Cells[row, 4, row, 5].Style.Font.Italic = true;
                    worksheetByClient.Cells[row, 4, row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheetByClient.Cells[row, 4, row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    row++;
                    //TOTAL AVEC COMISSION
                    worksheetByClient.Cells[row, 4].Value = "TOTAL";
                    worksheetByClient.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheetByClient.Cells[row, 5].Formula = new ExcelCellAddress(row - 2, 5).Address + "-" + new ExcelCellAddress(row - 1, 5).Address;
                    worksheetByClient.Cells[row, 5].Style.Numberformat.Format = "0.00€";
                    worksheetByClient.Cells[row, 4, row, 5].Style.Font.Size = 14;
                    worksheetByClient.Cells[row, 4, row, 5].Style.Font.Bold = true;
                    worksheetByClient.Cells[row, 4, row, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                    worksheetByClient.Cells[row, 4, row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    //☐
                    worksheetByClient.Cells[row, 6].Value = "☐";
                    worksheetByClient.Cells[row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheetByClient.Cells[row, 6].Style.Font.Size = 14;
                    //
                    row++;
                    row++;
                }
                row++;
                //Super Total
                totalWhitoutComissionFormula = "";
                totalComission = "";
                for (int cpt = 0; cpt < rowsTotal.Count; cpt++)
                {
                    totalWhitoutComissionFormula += new ExcelCellAddress(rowsTotal[cpt], 5).Address;
                    totalComission += new ExcelCellAddress(rowsTotal[cpt] + 1, 5).Address;
                    if (cpt != rowsTotal.Count - 1)
                    {
                        totalWhitoutComissionFormula += "+";
                        totalComission += "+";
                    }
                }
                worksheetByClient.Cells[row, 4].Value = "Total sans comission";
                worksheetByClient.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheetByClient.Cells[row, 5].Formula = totalWhitoutComissionFormula;
                worksheetByClient.Cells[row, 5].Style.Numberformat.Format = "0.00€";
                worksheetByClient.Cells[row, 4, row, 5].Style.Font.Italic = true;
                worksheetByClient.Cells[row, 4, row, 5].Style.Font.Bold = true;
                worksheetByClient.Cells[row, 4, row, 5].Style.Font.Size = 10;
                row++;
                worksheetByClient.Cells[row, 4].Value = "Total comission à " + Configurations.ApplicationConfig.Comission + "%";
                worksheetByClient.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheetByClient.Cells[row, 5].Formula = totalComission;
                worksheetByClient.Cells[row, 5].Style.Numberformat.Format = "0.00€";
                worksheetByClient.Cells[row, 4, row, 5].Style.Font.Italic = true;
                worksheetByClient.Cells[row, 4, row, 5].Style.Font.Bold = true;
                worksheetByClient.Cells[row, 4, row, 5].Style.Font.Size = 10;
                row++;
                worksheetByClient.Cells[row, 4].Value = "TOTAL :";
                worksheetByClient.Cells[row, 4].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                worksheetByClient.Cells[row, 5].Formula = new ExcelCellAddress(row - 2, 5).Address + "-" + new ExcelCellAddress(row - 1, 5).Address;
                worksheetByClient.Cells[row, 5].Style.Numberformat.Format = "0.00€";
                worksheetByClient.Cells[row, 4, row, 5].Style.Font.Bold = true;
                worksheetByClient.Cells[row, 4, row, 5].Style.Font.Size = 18;
                //


                //Disposition collone
                worksheetByClient.View.PageLayoutView = true;
                worksheetByClient.Column(1).Width = (161 - 12 + 5) / 7d + 1;
                worksheetByClient.Column(2).Width = (133 - 12 + 5) / 7d + 1;
                worksheetByClient.Column(3).Width = (83 - 12 + 5) / 7d + 1;
                worksheetByClient.Column(4).Width = (108 - 12 + 5) / 7d + 1;
                worksheetByClient.Column(5).Width = (83 - 12 + 5) / 7d + 1;
                worksheetByClient.Column(6).Width = (20 - 12 + 5) / 7d + 1;

                #endregion Facture par client


                // Document properties
                package.Workbook.Properties.Title = "Facture : " + bill.BillNumber;
                package.Workbook.Properties.Author = "Stolons";
                package.Workbook.Properties.Comments = "Facture de la semaine " + bill.BillNumber;

                // Extended property values
                package.Workbook.Properties.Company = "Association Stolons";

                // save our new workbook and we are done!
                package.Save();

            }
        

            //
            return bill;
        }

        private static ConsumerBill GenerateBill(ValidatedWeekBasket weekBasket, ApplicationDbContext dbContext)
        {
            ConsumerBill bill = CreateBill<ConsumerBill>(weekBasket.Consumer);
            //Generate exel file with bill number for user
            string consumerBillsPath = Path.Combine(Configurations.Environment.WebRootPath, Configurations.ConsumersBillsStockagePath, bill.User.Id.ToString());
	    string newBillPath = Path.Combine(consumerBillsPath,  bill.BillNumber + ".xlsx");
            FileInfo newFile = new FileInfo(newBillPath);
            if (newFile.Exists)
            {
                //Normaly impossible
                newFile.Delete();  // ensures we create a new workbook
		newFile = new FileInfo(newBillPath);
            }
            else
            {
                Directory.CreateDirectory(consumerBillsPath);
            }
            using (ExcelPackage package = new ExcelPackage(newFile))
            {
                // add a new worksheet to the empty workbook
                ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("Facture");
                //Add global informations
                int row = 1;
                worksheet.Cells[row, 1].Value = Configurations.ApplicationConfig.StolonsLabel;
                row++;
                worksheet.Cells[row, 1].Value = Configurations.ApplicationConfig.MailAddress;
                row++;
                worksheet.Cells[row, 1].Value = Configurations.ApplicationConfig.StolonsPhoneNumber;
                row++;
                worksheet.Cells[row, 1].Value = Configurations.ApplicationConfig.StolonsAddress;
                row++;
                worksheet.Cells[row, 5].Value = weekBasket.Consumer.Surname + ", " + weekBasket.Consumer.Name;
                row++;
                worksheet.Cells[row, 5].Value = weekBasket.Consumer.Email;
                row++;
                worksheet.Cells[row, 1].Value = "Numéro d'adhérent :";
                worksheet.Cells[row, 2].Value = weekBasket.Consumer.Id;
                worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                row++;
                worksheet.Cells[row, 1].Value = "Numéro de facture :";
                worksheet.Cells[row, 2].Value = bill.BillNumber;
                worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                row++;
                worksheet.Cells[row, 1].Value = "Année :";
                worksheet.Cells[row, 2].Value = DateTime.Now.Year;
                worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                row++;
                worksheet.Cells[row, 1].Value = "Semaine :";
                worksheet.Cells[row, 2].Value = DateTime.Now.GetIso8601WeekOfYear();
                worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                row++;
                worksheet.Cells[row, 1].Value = "Edité le :";
                worksheet.Cells[row, 2].Value = DateTime.Now.ToString();
                worksheet.Cells[row, 2].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                row++;
                row++;
                //Add product informations
                worksheet.Cells[row, 1, row + 1, 6].Merge = true;
                worksheet.Cells[row, 1, row + 1, 6].Value = "Produits de votre panier de la semaine";
                worksheet.Cells[row, 1, row + 1, 6].Style.Font.Bold = true;
                worksheet.Cells[row, 1, row + 1, 6].Style.Font.Size = 14;
                row++;
                row++;
                // - Add the headers
                worksheet.Cells[row, 1].Value = "NOM";
                worksheet.Cells[row, 2].Value = "TYPE";
                worksheet.Cells[row, 3].Value = "PRIX UNITAIRE";
                worksheet.Cells[row, 4].Value = "QUANTITE";
                worksheet.Cells[row, 5].Value = "";
                worksheet.Cells[row, 6].Value = "MONTANT";
                worksheet.Cells[row, 1, row, 6].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                worksheet.Cells[row, 1, row, 6].Style.Font.Bold = true;
                row++;
                int startRow = row;
                // - Add products
                foreach (var tmpBillEntry in weekBasket.Products)
                {
                    var billEntry = dbContext.BillEntrys.Include(x => x.Product).ThenInclude(x => x.Familly).First(x => x.Id == tmpBillEntry.Id);
                    worksheet.Cells[row, 1].Value = billEntry.Product.Name;
                    worksheet.Cells[row, 1].Style.Font.Bold = true;
                    string typeDetail = billEntry.Product.Type == Product.SellType.Piece ? "" : " par " + billEntry.Product.QuantityStepString;
                    worksheet.Cells[row, 2].Value = EnumHelper<Product.SellType>.GetDisplayValue(billEntry.Product.Type) + typeDetail;
                    worksheet.Cells[row, 3].Value = billEntry.Product.UnitPrice;
                    worksheet.Cells[row, 3].Style.Numberformat.Format = "0.00€";
                    worksheet.Cells[row, 4].Value = billEntry.Quantity;
                    worksheet.Cells[row, 5].Value = billEntry.QuantityString;
                    worksheet.Cells[row, 5].Style.HorizontalAlignment = ExcelHorizontalAlignment.Right;
                    worksheet.Cells[row, 6].Formula = new ExcelCellAddress(row,3).Address +"*" + new ExcelCellAddress(row, 4).Address;
                    worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00€";
                    row++;
                }
                worksheet.Cells[startRow - 1, 1, row - 1, 6].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                //- Add TOTAL
                worksheet.Cells[row, 5].Value = "TOTAL : ";
                worksheet.Cells[row, 5].Style.Font.Bold = true;

                //Add a formula for the value-column
                worksheet.Cells[row, 6].Formula = string.Format("SUBTOTAL(9,{0})", new ExcelAddress(startRow, 6, row -1, 6).Address);
                worksheet.Cells[row, 6].Style.Numberformat.Format = "0.00€";
                worksheet.Cells[row, 6].Style.Font.Bold = true;

                row++;
                row++;
                worksheet.Cells[row, 1].Value = Configurations.ApplicationConfig.OrderDeliveryMessage;
                worksheet.Cells[row, 1].Style.Font.Bold = true;

                // Document properties
                package.Workbook.Properties.Title = "Facture : " + bill.BillNumber;
                package.Workbook.Properties.Author = "Stolons";
                package.Workbook.Properties.Comments = "Facture de la semaine " + bill.BillNumber;

                // Extended property values
                package.Workbook.Properties.Company = "Association Stolons";
                //Column size
                worksheet.View.PageLayoutView = true;
                worksheet.Column(1).Width = (134 - 12 + 5) / 7d + 1;
                worksheet.Column(2).Width = (134 - 12 + 5) / 7d + 1;
                worksheet.Column(3).Width = (90 - 12 + 5) / 7d + 1;
                worksheet.Column(4).Width = (80 - 12 + 5) / 7d + 1;
                worksheet.Column(5).Width = (80 - 12 + 5) / 7d + 1;
                worksheet.Column(6).Width = (80 - 12 + 5) / 7d + 1;
                // save our new workbook and we are done!
                package.Save();
            }
            //
            return bill;
        }

        private static T CreateBill<T>(User user) where T : class, IBill , new()
        {
            IBill bill = new T();
            bill.BillNumber = DateTime.Now.Year + "_" + DateTime.Now.GetIso8601WeekOfYear() +"_" + user.Id;
            bill.User = user;
            bill.State = BillState.Pending;
            bill.EditionDate = DateTime.Now;
            return bill as T;
        }

        public static int GetIso8601WeekOfYear(this DateTime time)
        {
            // Seriously cheat.  If its Monday, Tuesday or Wednesday, then it'll 
            // be the same week# as whatever Thursday, Friday or Saturday are,
            // and we always get those right
            DayOfWeek day = CultureInfo.InvariantCulture.Calendar.GetDayOfWeek(time);
            if (day >= DayOfWeek.Monday && day <= DayOfWeek.Wednesday)
            {
                time = time.AddDays(3);
            }

            // Return the week of our adjusted day
            return CultureInfo.InvariantCulture.Calendar.GetWeekOfYear(time, CalendarWeekRule.FirstFullWeek, DayOfWeek.Monday);
        }
    }
}

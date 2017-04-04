using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Stolons.Models.Users;
using Stolons.Models.Messages;
using Stolons.Models.Transactions;

namespace Stolons.Models
{
    //In Nugget console
    //Add-Migration DbMigrations
    //Update-Database 
    //In console
    //dotnet ef migrations add DbMigrations 
    //dotnet ef database update
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        //Users
        public DbSet<Adherent> Adherents { get; set; }
        public DbSet<Sympathizer> Sympathizers { get; set; }
        //Message
        public DbSet<News> News { get; set; }
        //Bills
        public DbSet<ConsumerBill> ConsumerBills { get; set; }
        public DbSet<ProducerBill> ProducerBills { get; set; }
        public DbSet<BillEntry> BillEntrys { get; set; }
        public DbSet<StolonsBill> StolonsBills { get; set; }
        //Products
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductStockStolon> ProductsStocks { get; set; }
        public DbSet<ProductFamilly> ProductFamillys { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        //WeekBasket
        public DbSet<TempWeekBasket> TempsWeekBaskets { get; set; }
        public DbSet<ValidatedWeekBasket> ValidatedWeekBaskets { get; set; }
        //Stolons        
        public DbSet<Stolon> Stolons { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<ApplicationConfig> ApplicationConfig { get; set; }
        public DbSet<Transaction> Transactions { get; set; }
        public DbSet<AdherentTransaction> AdherentTransactions { get; set; }

        //Adherent Stolons
        public DbSet<AdherentStolon> AdherentStolons { get; set; }


        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {

        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            //AdherentStolon
            modelBuilder.Entity<AdherentStolon>()
                .HasOne(x => x.Adherent);
            modelBuilder.Entity<AdherentStolon>()
                .HasOne(x => x.Stolon);
            //Adherent
            modelBuilder.Entity<Adherent>()
                .HasMany(x => x.AdherentStolons)
                    .WithOne(x => x.Adherent);
            //StolonsStock
            modelBuilder.Entity<ProductStockStolon>()
                .HasOne(x => x.Product);
            //Product
            modelBuilder.Entity<Product>()
                .HasMany(x => x.BillEntries)
                    .WithOne(x => x.Product);
            modelBuilder.Entity<Product>()
                .HasOne(x => x.Producer);
        }
    }

    public static class DbTools
    {
        public static void Clear<T>(this DbSet<T> dbSet) where T : class
        {
            dbSet.RemoveRange(dbSet);
        }
    }
}

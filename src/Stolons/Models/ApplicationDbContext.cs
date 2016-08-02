using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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
        public DbSet<Sympathizer> StolonsUsers { get; set; }
        public DbSet<Producer> Producers { get; set; }
        public DbSet<Consumer> Consumers { get; set; }
        public DbSet<News> News { get; set; }
        public DbSet<ConsumerBill> ConsumerBills { get; set; }
        public DbSet<ProducerBill> ProducerBills { get; set; }
        public DbSet<BillEntry> BillEntrys { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductFamilly> ProductFamillys { get; set; }
        public DbSet<ProductType> ProductTypes { get; set; }
        public DbSet<TempWeekBasket> TempsWeekBaskets { get; set; }
        public DbSet<ValidatedWeekBasket> ValidatedWeekBaskets { get; set; }
        public DbSet<ApplicationConfig> ApplicationConfig { get; set; }
        public DbSet<Transaction> Transactions { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Stolons.Models
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public DbSet<User> StolonsUsers { get; set; }
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

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
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

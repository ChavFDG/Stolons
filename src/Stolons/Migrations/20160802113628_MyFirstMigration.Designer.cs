using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using Stolons.Models;

namespace Stolons.Migrations
{
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20160802113628_MyFirstMigration")]
    partial class MyFirstMigration
    {
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
            modelBuilder
                .HasAnnotation("ProductVersion", "1.0.0-rtm-21431");

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole", b =>
                {
                    b.Property<string>("Id");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Name")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedName")
                        .HasName("RoleNameIndex");

                    b.ToTable("AspNetRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("RoleId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("RoleId");

                    b.ToTable("AspNetRoleClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("ClaimType");

                    b.Property<string>("ClaimValue");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("Id");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserClaims");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.Property<string>("LoginProvider");

                    b.Property<string>("ProviderKey");

                    b.Property<string>("ProviderDisplayName");

                    b.Property<string>("UserId")
                        .IsRequired();

                    b.HasKey("LoginProvider", "ProviderKey");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserLogins");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("RoleId");

                    b.HasKey("UserId", "RoleId");

                    b.HasIndex("RoleId");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUserRoles");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserToken<string>", b =>
                {
                    b.Property<string>("UserId");

                    b.Property<string>("LoginProvider");

                    b.Property<string>("Name");

                    b.Property<string>("Value");

                    b.HasKey("UserId", "LoginProvider", "Name");

                    b.ToTable("AspNetUserTokens");
                });

            modelBuilder.Entity("Stolons.Models.ApplicationConfig", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Comission");

                    b.Property<int>("ConsumerSubscription");

                    b.Property<string>("ContactMailAddress");

                    b.Property<int>("DeliveryAndStockUpdateDayStartDate");

                    b.Property<int>("DeliveryAndStockUpdateDayStartDateHourStartDate");

                    b.Property<int>("DeliveryAndStockUpdateDayStartDateMinuteStartDate");

                    b.Property<bool>("IsModeSimulated");

                    b.Property<string>("MailAddress");

                    b.Property<string>("MailPassword");

                    b.Property<int>("MailPort");

                    b.Property<string>("MailSmtp");

                    b.Property<int>("OrderDayStartDate");

                    b.Property<string>("OrderDeliveryMessage");

                    b.Property<int>("OrderHourStartDate");

                    b.Property<int>("OrderMinuteStartDate");

                    b.Property<int>("ProducerSubscription");

                    b.Property<int>("SimulationMode");

                    b.Property<string>("StolonsAboutPageText");

                    b.Property<string>("StolonsAddress");

                    b.Property<string>("StolonsLabel");

                    b.Property<string>("StolonsPhoneNumber");

                    b.Property<int>("SubscriptionStartMonth");

                    b.Property<int>("SympathizerSubscription");

                    b.HasKey("Id");

                    b.ToTable("ApplicationConfig");
                });

            modelBuilder.Entity("Stolons.Models.ApplicationUser", b =>
                {
                    b.Property<string>("Id");

                    b.Property<int>("AccessFailedCount");

                    b.Property<string>("ConcurrencyStamp")
                        .IsConcurrencyToken();

                    b.Property<string>("Email")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<bool>("EmailConfirmed");

                    b.Property<bool>("LockoutEnabled");

                    b.Property<DateTimeOffset?>("LockoutEnd");

                    b.Property<string>("NormalizedEmail")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("NormalizedUserName")
                        .HasAnnotation("MaxLength", 256);

                    b.Property<string>("PasswordHash");

                    b.Property<string>("PhoneNumber");

                    b.Property<bool>("PhoneNumberConfirmed");

                    b.Property<string>("SecurityStamp");

                    b.Property<bool>("TwoFactorEnabled");

                    b.Property<int?>("UserId");

                    b.Property<string>("UserName")
                        .HasAnnotation("MaxLength", 256);

                    b.HasKey("Id");

                    b.HasIndex("NormalizedEmail")
                        .HasName("EmailIndex");

                    b.HasIndex("NormalizedUserName")
                        .IsUnique()
                        .HasName("UserNameIndex");

                    b.HasIndex("UserId");

                    b.ToTable("AspNetUsers");
                });

            modelBuilder.Entity("Stolons.Models.BillEntry", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<Guid>("ProductId");

                    b.Property<int>("Quantity");

                    b.Property<Guid?>("TempWeekBasketId");

                    b.Property<Guid?>("ValidatedWeekBasketId");

                    b.HasKey("Id");

                    b.HasIndex("ProductId");

                    b.HasIndex("TempWeekBasketId");

                    b.HasIndex("ValidatedWeekBasketId");

                    b.ToTable("BillEntrys");
                });

            modelBuilder.Entity("Stolons.Models.ConsumerBill", b =>
                {
                    b.Property<string>("BillNumber");

                    b.Property<int?>("ConsumerId");

                    b.Property<DateTime>("EditionDate");

                    b.Property<int>("State");

                    b.HasKey("BillNumber");

                    b.HasIndex("ConsumerId");

                    b.ToTable("ConsumerBills");
                });

            modelBuilder.Entity("Stolons.Models.News", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<DateTime>("DateOfPublication");

                    b.Property<string>("ImageLink");

                    b.Property<string>("Message")
                        .IsRequired();

                    b.Property<string>("Title")
                        .IsRequired();

                    b.Property<int>("UserForeignKey");

                    b.HasKey("Id");

                    b.HasIndex("UserForeignKey");

                    b.ToTable("News");
                });

            modelBuilder.Entity("Stolons.Models.ProducerBill", b =>
                {
                    b.Property<string>("BillNumber");

                    b.Property<DateTime>("EditionDate");

                    b.Property<int?>("ProducerId");

                    b.Property<int>("State");

                    b.HasKey("BillNumber");

                    b.HasIndex("ProducerId");

                    b.ToTable("ProducerBills");
                });

            modelBuilder.Entity("Stolons.Models.Product", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("AverageQuantity");

                    b.Property<DateTime>("DLC");

                    b.Property<string>("Description");

                    b.Property<string>("FamillyName");

                    b.Property<string>("LabelsSerialized");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("PicturesSerialized");

                    b.Property<float>("Price");

                    b.Property<int?>("ProducerId");

                    b.Property<int>("ProductUnit");

                    b.Property<int>("QuantityStep");

                    b.Property<float>("RemainingStock");

                    b.Property<int>("State");

                    b.Property<int>("Type");

                    b.Property<float>("UnitPrice");

                    b.Property<float>("WeekStock");

                    b.HasKey("Id");

                    b.HasIndex("FamillyName");

                    b.HasIndex("ProducerId");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Stolons.Models.ProductFamilly", b =>
                {
                    b.Property<string>("FamillyName");

                    b.Property<string>("Image");

                    b.Property<string>("TypeName");

                    b.HasKey("FamillyName");

                    b.HasIndex("TypeName");

                    b.ToTable("ProductFamillys");
                });

            modelBuilder.Entity("Stolons.Models.ProductType", b =>
                {
                    b.Property<string>("Name");

                    b.Property<string>("Image");

                    b.HasKey("Name");

                    b.ToTable("ProductTypes");
                });

            modelBuilder.Entity("Stolons.Models.SympathizerUser", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<string>("Address");

                    b.Property<string>("Avatar");

                    b.Property<string>("City");

                    b.Property<bool>("Cotisation");

                    b.Property<string>("DisableReason");

                    b.Property<string>("Discriminator")
                        .IsRequired();

                    b.Property<string>("Email");

                    b.Property<bool>("Enable");

                    b.Property<string>("Name")
                        .IsRequired();

                    b.Property<string>("PhoneNumber");

                    b.Property<string>("PostCode");

                    b.Property<DateTime>("RegistrationDate");

                    b.Property<string>("Surname")
                        .IsRequired();

                    b.HasKey("Id");

                    b.ToTable("StolonsUsers");

                    b.HasDiscriminator<string>("Discriminator").HasValue("SympathizerUser");
                });

            modelBuilder.Entity("Stolons.Models.TempWeekBasket", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ConsumerId");

                    b.Property<bool>("Validated");

                    b.HasKey("Id");

                    b.HasIndex("ConsumerId");

                    b.ToTable("TempsWeekBaskets");
                });

            modelBuilder.Entity("Stolons.Models.Transaction", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int>("Amount");

                    b.Property<int>("Category");

                    b.Property<DateTime>("Date");

                    b.Property<string>("Description");

                    b.Property<int>("Type");

                    b.HasKey("Id");

                    b.ToTable("Transactions");
                });

            modelBuilder.Entity("Stolons.Models.ValidatedWeekBasket", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd();

                    b.Property<int?>("ConsumerId");

                    b.HasKey("Id");

                    b.HasIndex("ConsumerId");

                    b.ToTable("ValidatedWeekBaskets");
                });

            modelBuilder.Entity("Stolons.Models.Consumer", b =>
                {
                    b.HasBaseType("Stolons.Models.SympathizerUser");


                    b.ToTable("Consumer");

                    b.HasDiscriminator().HasValue("Consumer");
                });

            modelBuilder.Entity("Stolons.Models.Producer", b =>
                {
                    b.HasBaseType("Stolons.Models.SympathizerUser");

                    b.Property<int>("Area");

                    b.Property<string>("CompanyName");

                    b.Property<string>("ExploitationPicuresSerialized");

                    b.Property<double>("Latitude");

                    b.Property<double>("Longitude");

                    b.Property<string>("OpenText");

                    b.Property<string>("Production");

                    b.Property<int>("StartDate");

                    b.Property<string>("WebSiteLink");

                    b.ToTable("Producer");

                    b.HasDiscriminator().HasValue("Producer");
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRoleClaim<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Claims")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserClaim<string>", b =>
                {
                    b.HasOne("Stolons.Models.ApplicationUser")
                        .WithMany("Claims")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserLogin<string>", b =>
                {
                    b.HasOne("Stolons.Models.ApplicationUser")
                        .WithMany("Logins")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityUserRole<string>", b =>
                {
                    b.HasOne("Microsoft.AspNetCore.Identity.EntityFrameworkCore.IdentityRole")
                        .WithMany("Users")
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Stolons.Models.ApplicationUser")
                        .WithMany("Roles")
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Stolons.Models.ApplicationUser", b =>
                {
                    b.HasOne("Stolons.Models.SympathizerUser", "User")
                        .WithMany()
                        .HasForeignKey("UserId");
                });

            modelBuilder.Entity("Stolons.Models.BillEntry", b =>
                {
                    b.HasOne("Stolons.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("Stolons.Models.TempWeekBasket")
                        .WithMany("Products")
                        .HasForeignKey("TempWeekBasketId");

                    b.HasOne("Stolons.Models.ValidatedWeekBasket")
                        .WithMany("Products")
                        .HasForeignKey("ValidatedWeekBasketId");
                });

            modelBuilder.Entity("Stolons.Models.ConsumerBill", b =>
                {
                    b.HasOne("Stolons.Models.Consumer", "Consumer")
                        .WithMany("ConsumerBills")
                        .HasForeignKey("ConsumerId");
                });

            modelBuilder.Entity("Stolons.Models.News", b =>
                {
                    b.HasOne("Stolons.Models.SympathizerUser", "User")
                        .WithMany("News")
                        .HasForeignKey("UserForeignKey")
                        .OnDelete(DeleteBehavior.Cascade);
                });

            modelBuilder.Entity("Stolons.Models.ProducerBill", b =>
                {
                    b.HasOne("Stolons.Models.Producer", "Producer")
                        .WithMany("ProducerBills")
                        .HasForeignKey("ProducerId");
                });

            modelBuilder.Entity("Stolons.Models.Product", b =>
                {
                    b.HasOne("Stolons.Models.ProductFamilly", "Familly")
                        .WithMany()
                        .HasForeignKey("FamillyName");

                    b.HasOne("Stolons.Models.Producer", "Producer")
                        .WithMany()
                        .HasForeignKey("ProducerId");
                });

            modelBuilder.Entity("Stolons.Models.ProductFamilly", b =>
                {
                    b.HasOne("Stolons.Models.ProductType", "Type")
                        .WithMany("ProductFamilly")
                        .HasForeignKey("TypeName");
                });

            modelBuilder.Entity("Stolons.Models.TempWeekBasket", b =>
                {
                    b.HasOne("Stolons.Models.Consumer", "Consumer")
                        .WithMany()
                        .HasForeignKey("ConsumerId");
                });

            modelBuilder.Entity("Stolons.Models.ValidatedWeekBasket", b =>
                {
                    b.HasOne("Stolons.Models.Consumer", "Consumer")
                        .WithMany()
                        .HasForeignKey("ConsumerId");
                });
        }
    }
}

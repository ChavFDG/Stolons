using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Stolons.Migrations
{
    public partial class DbMigrationRememberBase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            return;
            migrationBuilder.CreateTable(
                name: "Adherents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Address = table.Column<string>(nullable: true),
                    Area = table.Column<decimal>(nullable: false),
                    AvatarFileName = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    CompanyName = table.Column<string>(nullable: true),
                    Email = table.Column<string>(nullable: true),
                    ExploitationPicuresSerialized = table.Column<string>(nullable: true),
                    IsWebAdmin = table.Column<bool>(nullable: false),
                    Latitude = table.Column<double>(nullable: false),
                    Longitude = table.Column<double>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    OpenText = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PostCode = table.Column<string>(nullable: false),
                    Production = table.Column<string>(nullable: true),
                    ReceivedGoodPlanByEmail = table.Column<bool>(nullable: false),
                    ReceivedInformationsEmail = table.Column<bool>(nullable: false),
                    ReceivedProductListByEmail = table.Column<bool>(nullable: false),
                    SellerType = table.Column<int>(nullable: false),
                    StartDate = table.Column<int>(nullable: false),
                    Surname = table.Column<string>(nullable: false),
                    WebSiteLink = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Adherents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ApplicationConfig",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ChromiumFullPath = table.Column<string>(nullable: true),
                    ContactMailAddress = table.Column<string>(nullable: true),
                    ContactPhoneNumber = table.Column<string>(nullable: true),
                    IsInMaintenance = table.Column<bool>(nullable: false),
                    MailAddress = table.Column<string>(nullable: true),
                    MailPassword = table.Column<string>(nullable: true),
                    MailPort = table.Column<int>(nullable: false),
                    MailSmtp = table.Column<string>(nullable: true),
                    MaintenanceMessage = table.Column<string>(nullable: true),
                    StolonsLabel = table.Column<string>(nullable: true),
                    StolonsUrl = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Name = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CanBeRemoved = table.Column<bool>(nullable: false),
                    Image = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Stolons",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AboutText = table.Column<string>(nullable: true),
                    Address = table.Column<string>(nullable: true),
                    BasketPickEndUpDay = table.Column<int>(nullable: false),
                    BasketPickUpEndHour = table.Column<int>(nullable: false),
                    BasketPickUpEndMinute = table.Column<int>(nullable: false),
                    BasketPickUpStartDay = table.Column<int>(nullable: false),
                    BasketPickUpStartHour = table.Column<int>(nullable: false),
                    BasketPickUpStartMinute = table.Column<int>(nullable: false),
                    ConsumerSubscription = table.Column<decimal>(nullable: false),
                    ContactMailAddress = table.Column<string>(nullable: true),
                    CreationDate = table.Column<DateTime>(nullable: false),
                    DefaultProducersFee = table.Column<int>(nullable: false),
                    DeliveryAndStockUpdateDayStartDate = table.Column<int>(nullable: false),
                    DeliveryAndStockUpdateDayStartDateHourStartDate = table.Column<int>(nullable: false),
                    DeliveryAndStockUpdateDayStartDateMinuteStartDate = table.Column<int>(nullable: false),
                    FacebookPage = table.Column<string>(nullable: true),
                    GoodPlan = table.Column<bool>(nullable: false),
                    IsModeSimulated = table.Column<bool>(nullable: false),
                    JoinUsText = table.Column<string>(nullable: true),
                    Label = table.Column<string>(nullable: true),
                    Latitude = table.Column<double>(nullable: false),
                    LogoFileName = table.Column<string>(nullable: true),
                    Longitude = table.Column<double>(nullable: false),
                    OrderDayStartDate = table.Column<int>(nullable: false),
                    OrderDeliveryMessage = table.Column<string>(nullable: true),
                    OrderHourStartDate = table.Column<int>(nullable: false),
                    OrderMinuteStartDate = table.Column<int>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: true),
                    ProducerSubscription = table.Column<decimal>(nullable: false),
                    ShortLabel = table.Column<string>(nullable: true),
                    SimulationMode = table.Column<int>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    StolonStateMessage = table.Column<string>(nullable: true),
                    StolonType = table.Column<int>(nullable: false),
                    SubscriptionPaid = table.Column<bool>(nullable: false),
                    SubscriptionStartMonth = table.Column<int>(nullable: false),
                    SympathizerSubscription = table.Column<decimal>(nullable: false),
                    UseHalftSubscipstion = table.Column<bool>(nullable: false),
                    UseProducersFee = table.Column<bool>(nullable: false),
                    UseSubscipstion = table.Column<bool>(nullable: false),
                    UseSympathizer = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Stolons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(nullable: false),
                    AccessFailedCount = table.Column<int>(nullable: false),
                    ConcurrencyStamp = table.Column<string>(nullable: true),
                    Email = table.Column<string>(maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(nullable: false),
                    LockoutEnabled = table.Column<bool>(nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(nullable: true),
                    NormalizedEmail = table.Column<string>(maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(maxLength: 256, nullable: true),
                    PasswordHash = table.Column<string>(nullable: true),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(nullable: false),
                    SecurityStamp = table.Column<string>(nullable: true),
                    TwoFactorEnabled = table.Column<bool>(nullable: false),
                    UserId = table.Column<Guid>(nullable: true),
                    UserName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Adherents_UserId",
                        column: x => x.UserId,
                        principalTable: "Adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductFamillys",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    CanBeRemoved = table.Column<bool>(nullable: false),
                    FamillyName = table.Column<string>(nullable: true),
                    Image = table.Column<string>(nullable: true),
                    TypeId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFamillys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductFamillys_ProductTypes_TypeId",
                        column: x => x.TypeId,
                        principalTable: "ProductTypes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AdherentStolons",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AdherentId = table.Column<Guid>(nullable: false),
                    Deleted = table.Column<bool>(nullable: false),
                    DisableReason = table.Column<string>(nullable: true),
                    Enable = table.Column<bool>(nullable: false),
                    IsActiveStolon = table.Column<bool>(nullable: false),
                    IsProducer = table.Column<bool>(nullable: false),
                    LocalId = table.Column<int>(nullable: false),
                    ProducerFee = table.Column<int>(nullable: false),
                    RegistrationDate = table.Column<DateTime>(nullable: false),
                    Role = table.Column<int>(nullable: false),
                    StolonId = table.Column<Guid>(nullable: true),
                    SubscriptionPaid = table.Column<bool>(nullable: false),
                    Token = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AdherentStolons", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AdherentStolons_Adherents_AdherentId",
                        column: x => x.AdherentId,
                        principalTable: "Adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AdherentStolons_Stolons_StolonId",
                        column: x => x.StolonId,
                        principalTable: "Stolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    ImageName = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: true),
                    StolonId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Services_Stolons_StolonId",
                        column: x => x.StolonId,
                        principalTable: "Stolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "StolonsBills",
                columns: table => new
                {
                    StolonBillId = table.Column<Guid>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    BillNumber = table.Column<string>(nullable: true),
                    Consumers = table.Column<int>(nullable: false),
                    EditionDate = table.Column<DateTime>(nullable: false),
                    FeeAmount = table.Column<decimal>(nullable: false),
                    HasBeenModified = table.Column<bool>(nullable: false),
                    HtmlBillContent = table.Column<string>(nullable: true),
                    ModificationReason = table.Column<string>(nullable: true),
                    Producers = table.Column<int>(nullable: false),
                    StolonId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StolonsBills", x => x.StolonBillId);
                    table.ForeignKey(
                        name: "FK_StolonsBills_Stolons_StolonId",
                        column: x => x.StolonId,
                        principalTable: "Stolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Sympathizers",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    PostCode = table.Column<string>(nullable: false),
                    ReceivedInformationsEmail = table.Column<bool>(nullable: false),
                    RegistrationDate = table.Column<DateTime>(nullable: false),
                    StolonId = table.Column<Guid>(nullable: false),
                    SubscriptionPaid = table.Column<bool>(nullable: false),
                    Surname = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sympathizers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sympathizers_Stolons_StolonId",
                        column: x => x.StolonId,
                        principalTable: "Stolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    AdherentId = table.Column<Guid>(nullable: true),
                    Id = table.Column<Guid>(nullable: false),
                    AddedAutomaticly = table.Column<bool>(nullable: false),
                    Amount = table.Column<decimal>(nullable: false),
                    Category = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    StolonId = table.Column<Guid>(nullable: false),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_Adherents_AdherentId",
                        column: x => x.AdherentId,
                        principalTable: "Adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_Stolons_StolonId",
                        column: x => x.StolonId,
                        principalTable: "Stolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.SerialColumn),
                    ClaimType = table.Column<string>(nullable: true),
                    ClaimValue = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(nullable: false),
                    ProviderKey = table.Column<string>(nullable: false),
                    ProviderDisplayName = table.Column<string>(nullable: true),
                    UserId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    RoleId = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(nullable: false),
                    LoginProvider = table.Column<string>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    Value = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AverageQuantity = table.Column<int>(nullable: false),
                    DLC = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    FamillyId = table.Column<Guid>(nullable: false),
                    HideWeightPrice = table.Column<bool>(nullable: false),
                    IsArchive = table.Column<bool>(nullable: false),
                    IsModified = table.Column<bool>(nullable: false),
                    LabelsSerialized = table.Column<string>(nullable: true),
                    MaximumWeight = table.Column<decimal>(nullable: false),
                    MinimumWeight = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    PicturesSerialized = table.Column<string>(nullable: true),
                    ProducerId = table.Column<Guid>(nullable: false),
                    ProductUnit = table.Column<int>(nullable: false),
                    QuantityStep = table.Column<int>(nullable: false),
                    StockManagement = table.Column<int>(nullable: false),
                    Storage = table.Column<int>(nullable: false),
                    Tax = table.Column<decimal>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    UnitPrice = table.Column<decimal>(nullable: false),
                    WeightPrice = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_ProductFamillys_FamillyId",
                        column: x => x.FamillyId,
                        principalTable: "ProductFamillys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Products_Adherents_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "Adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ChatMessages",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    DateOfPublication = table.Column<DateTime>(nullable: false),
                    PublishByAdherentStolonId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatMessages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ChatMessages_AdherentStolons_PublishByAdherentStolonId",
                        column: x => x.PublishByAdherentStolonId,
                        principalTable: "AdherentStolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConsumerBills",
                columns: table => new
                {
                    BillId = table.Column<Guid>(nullable: false),
                    AdherentId = table.Column<Guid>(nullable: true),
                    AdherentStolonId = table.Column<Guid>(nullable: true),
                    BillNumber = table.Column<string>(nullable: true),
                    EditionDate = table.Column<DateTime>(nullable: false),
                    HasBeenModified = table.Column<bool>(nullable: false),
                    HtmlBillContent = table.Column<string>(nullable: true),
                    ModificationReason = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    OrderAmount = table.Column<decimal>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    TokenUsed = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerBills", x => x.BillId);
                    table.ForeignKey(
                        name: "FK_ConsumerBills_Adherents_AdherentId",
                        column: x => x.AdherentId,
                        principalTable: "Adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsumerBills_AdherentStolons_AdherentStolonId",
                        column: x => x.AdherentStolonId,
                        principalTable: "AdherentStolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Content = table.Column<string>(nullable: false),
                    DateOfPublication = table.Column<DateTime>(nullable: false),
                    ImageName = table.Column<string>(nullable: true),
                    IsHighlight = table.Column<bool>(nullable: false),
                    PublishAs = table.Column<int>(nullable: false),
                    PublishByAdherentStolonId = table.Column<Guid>(nullable: false),
                    PublishEnd = table.Column<DateTime>(nullable: false),
                    PublishStart = table.Column<DateTime>(nullable: false),
                    Title = table.Column<string>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                    table.ForeignKey(
                        name: "FK_News_AdherentStolons_PublishByAdherentStolonId",
                        column: x => x.PublishByAdherentStolonId,
                        principalTable: "AdherentStolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProducerBills",
                columns: table => new
                {
                    BillId = table.Column<Guid>(nullable: false),
                    AdherentId = table.Column<Guid>(nullable: true),
                    AdherentStolonId = table.Column<Guid>(nullable: true),
                    BillNumber = table.Column<string>(nullable: true),
                    EditionDate = table.Column<DateTime>(nullable: false),
                    HasBeenModified = table.Column<bool>(nullable: false),
                    HtmlBillContent = table.Column<string>(nullable: true),
                    HtmlOrderContent = table.Column<string>(nullable: true),
                    ModificationReason = table.Column<string>(nullable: true),
                    ModifiedDate = table.Column<DateTime>(nullable: false),
                    OrderAmount = table.Column<decimal>(nullable: false),
                    OrderNumber = table.Column<string>(nullable: true),
                    ProducerFee = table.Column<int>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    TaxAmount = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerBills", x => x.BillId);
                    table.ForeignKey(
                        name: "FK_ProducerBills_Adherents_AdherentId",
                        column: x => x.AdherentId,
                        principalTable: "Adherents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ProducerBills_AdherentStolons_AdherentStolonId",
                        column: x => x.AdherentStolonId,
                        principalTable: "AdherentStolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TempsWeekBaskets",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AdherentStolonId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempsWeekBaskets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TempsWeekBaskets_AdherentStolons_AdherentStolonId",
                        column: x => x.AdherentStolonId,
                        principalTable: "AdherentStolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ValidatedWeekBaskets",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AdherentStolonId = table.Column<Guid>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidatedWeekBaskets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidatedWeekBaskets_AdherentStolons_AdherentStolonId",
                        column: x => x.AdherentStolonId,
                        principalTable: "AdherentStolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProductsStocks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AdherentStolonId = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    RemainingStock = table.Column<decimal>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    WeekStock = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductsStocks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ProductsStocks_AdherentStolons_AdherentStolonId",
                        column: x => x.AdherentStolonId,
                        principalTable: "AdherentStolons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProductsStocks_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BillEntrys",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ConsumerBillId = table.Column<Guid>(nullable: true),
                    DLC = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    FamillyId = table.Column<Guid>(nullable: false),
                    HasBeenModified = table.Column<bool>(nullable: false),
                    IsModified = table.Column<bool>(nullable: false),
                    LabelsSerialized = table.Column<string>(nullable: true),
                    MaximumWeight = table.Column<decimal>(nullable: false),
                    MinimumWeight = table.Column<decimal>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    PicturesSerialized = table.Column<string>(nullable: true),
                    ProducerBillId = table.Column<Guid>(nullable: true),
                    ProducerFee = table.Column<int>(nullable: false),
                    ProductStockId = table.Column<Guid>(nullable: false),
                    ProductUnit = table.Column<int>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    QuantityStep = table.Column<int>(nullable: false),
                    StockManagement = table.Column<int>(nullable: false),
                    StolonsBillId = table.Column<Guid>(nullable: true),
                    Storage = table.Column<int>(nullable: false),
                    Tax = table.Column<decimal>(nullable: false),
                    TempWeekBasketId = table.Column<Guid>(nullable: true),
                    Type = table.Column<int>(nullable: false),
                    UnitPrice = table.Column<decimal>(nullable: false),
                    ValidatedWeekBasketId = table.Column<Guid>(nullable: true),
                    WeightAssigned = table.Column<bool>(nullable: false),
                    WeightPrice = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillEntrys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillEntrys_ConsumerBills_ConsumerBillId",
                        column: x => x.ConsumerBillId,
                        principalTable: "ConsumerBills",
                        principalColumn: "BillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillEntrys_ProductFamillys_FamillyId",
                        column: x => x.FamillyId,
                        principalTable: "ProductFamillys",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillEntrys_ProducerBills_ProducerBillId",
                        column: x => x.ProducerBillId,
                        principalTable: "ProducerBills",
                        principalColumn: "BillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillEntrys_ProductsStocks_ProductStockId",
                        column: x => x.ProductStockId,
                        principalTable: "ProductsStocks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BillEntrys_StolonsBills_StolonsBillId",
                        column: x => x.StolonsBillId,
                        principalTable: "StolonsBills",
                        principalColumn: "StolonBillId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillEntrys_TempsWeekBaskets_TempWeekBasketId",
                        column: x => x.TempWeekBasketId,
                        principalTable: "TempsWeekBaskets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BillEntrys_ValidatedWeekBaskets_ValidatedWeekBasketId",
                        column: x => x.ValidatedWeekBasketId,
                        principalTable: "ValidatedWeekBaskets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AdherentStolons_AdherentId",
                table: "AdherentStolons",
                column: "AdherentId");

            migrationBuilder.CreateIndex(
                name: "IX_AdherentStolons_StolonId",
                table: "AdherentStolons",
                column: "StolonId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserId",
                table: "AspNetUsers",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_BillEntrys_ConsumerBillId",
                table: "BillEntrys",
                column: "ConsumerBillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillEntrys_FamillyId",
                table: "BillEntrys",
                column: "FamillyId");

            migrationBuilder.CreateIndex(
                name: "IX_BillEntrys_ProducerBillId",
                table: "BillEntrys",
                column: "ProducerBillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillEntrys_ProductStockId",
                table: "BillEntrys",
                column: "ProductStockId");

            migrationBuilder.CreateIndex(
                name: "IX_BillEntrys_StolonsBillId",
                table: "BillEntrys",
                column: "StolonsBillId");

            migrationBuilder.CreateIndex(
                name: "IX_BillEntrys_TempWeekBasketId",
                table: "BillEntrys",
                column: "TempWeekBasketId");

            migrationBuilder.CreateIndex(
                name: "IX_BillEntrys_ValidatedWeekBasketId",
                table: "BillEntrys",
                column: "ValidatedWeekBasketId");

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_PublishByAdherentStolonId",
                table: "ChatMessages",
                column: "PublishByAdherentStolonId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerBills_AdherentId",
                table: "ConsumerBills",
                column: "AdherentId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerBills_AdherentStolonId",
                table: "ConsumerBills",
                column: "AdherentStolonId");

            migrationBuilder.CreateIndex(
                name: "IX_News_PublishByAdherentStolonId",
                table: "News",
                column: "PublishByAdherentStolonId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerBills_AdherentId",
                table: "ProducerBills",
                column: "AdherentId");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerBills_AdherentStolonId",
                table: "ProducerBills",
                column: "AdherentStolonId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFamillys_TypeId",
                table: "ProductFamillys",
                column: "TypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_FamillyId",
                table: "Products",
                column: "FamillyId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProducerId",
                table: "Products",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsStocks_AdherentStolonId",
                table: "ProductsStocks",
                column: "AdherentStolonId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductsStocks_ProductId",
                table: "ProductsStocks",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_Services_StolonId",
                table: "Services",
                column: "StolonId");

            migrationBuilder.CreateIndex(
                name: "IX_StolonsBills_StolonId",
                table: "StolonsBills",
                column: "StolonId");

            migrationBuilder.CreateIndex(
                name: "IX_Sympathizers_StolonId",
                table: "Sympathizers",
                column: "StolonId");

            migrationBuilder.CreateIndex(
                name: "IX_TempsWeekBaskets_AdherentStolonId",
                table: "TempsWeekBaskets",
                column: "AdherentStolonId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_AdherentId",
                table: "Transactions",
                column: "AdherentId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_StolonId",
                table: "Transactions",
                column: "StolonId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidatedWeekBaskets_AdherentStolonId",
                table: "ValidatedWeekBaskets",
                column: "AdherentStolonId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ApplicationConfig");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "BillEntrys");

            migrationBuilder.DropTable(
                name: "ChatMessages");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.DropTable(
                name: "Sympathizers");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "ConsumerBills");

            migrationBuilder.DropTable(
                name: "ProducerBills");

            migrationBuilder.DropTable(
                name: "ProductsStocks");

            migrationBuilder.DropTable(
                name: "StolonsBills");

            migrationBuilder.DropTable(
                name: "TempsWeekBaskets");

            migrationBuilder.DropTable(
                name: "ValidatedWeekBaskets");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "AdherentStolons");

            migrationBuilder.DropTable(
                name: "ProductFamillys");

            migrationBuilder.DropTable(
                name: "Adherents");

            migrationBuilder.DropTable(
                name: "Stolons");

            migrationBuilder.DropTable(
                name: "ProductTypes");
        }
    }
}

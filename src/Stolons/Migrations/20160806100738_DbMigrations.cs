using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Stolons.Migrations
{
    public partial class DbMigrations : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                });

            migrationBuilder.CreateTable(
                name: "ApplicationConfig",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ConsumerSubscription = table.Column<double>(nullable: false),
                    ContactMailAddress = table.Column<string>(nullable: true),
                    DeliveryAndStockUpdateDayStartDate = table.Column<int>(nullable: false),
                    DeliveryAndStockUpdateDayStartDateHourStartDate = table.Column<int>(nullable: false),
                    DeliveryAndStockUpdateDayStartDateMinuteStartDate = table.Column<int>(nullable: false),
                    Fee = table.Column<int>(nullable: false),
                    IsModeSimulated = table.Column<bool>(nullable: false),
                    MailAddress = table.Column<string>(nullable: true),
                    MailPassword = table.Column<string>(nullable: true),
                    MailPort = table.Column<int>(nullable: false),
                    MailSmtp = table.Column<string>(nullable: true),
                    OrderDayStartDate = table.Column<int>(nullable: false),
                    OrderDeliveryMessage = table.Column<string>(nullable: true),
                    OrderHourStartDate = table.Column<int>(nullable: false),
                    OrderMinuteStartDate = table.Column<int>(nullable: false),
                    ProducerSubscription = table.Column<double>(nullable: false),
                    SimulationMode = table.Column<int>(nullable: false),
                    StolonsAboutPageText = table.Column<string>(nullable: true),
                    StolonsAddress = table.Column<string>(nullable: true),
                    StolonsLabel = table.Column<string>(nullable: true),
                    StolonsPhoneNumber = table.Column<string>(nullable: true),
                    SubscriptionStartMonth = table.Column<int>(nullable: false),
                    SympathizerSubscription = table.Column<double>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ApplicationConfig", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ProductTypes",
                columns: table => new
                {
                    Name = table.Column<string>(nullable: false),
                    Image = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductTypes", x => x.Name);
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AddedAutomaticly = table.Column<bool>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    Category = table.Column<int>(nullable: false),
                    Date = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    Type = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "StolonsUsers",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
                    Address = table.Column<string>(nullable: true),
                    Avatar = table.Column<string>(nullable: true),
                    City = table.Column<string>(nullable: true),
                    Cotisation = table.Column<bool>(nullable: false),
                    DisableReason = table.Column<string>(nullable: true),
                    Discriminator = table.Column<string>(nullable: false),
                    Email = table.Column<string>(nullable: true),
                    Enable = table.Column<bool>(nullable: false),
                    Name = table.Column<string>(nullable: false),
                    PhoneNumber = table.Column<string>(nullable: true),
                    PostCode = table.Column<string>(nullable: false),
                    RegistrationDate = table.Column<DateTime>(nullable: false),
                    Surname = table.Column<string>(nullable: false),
                    Area = table.Column<int>(nullable: true),
                    CompanyName = table.Column<string>(nullable: true),
                    ExploitationPicuresSerialized = table.Column<string>(nullable: true),
                    Latitude = table.Column<double>(nullable: true),
                    Longitude = table.Column<double>(nullable: true),
                    OpenText = table.Column<string>(nullable: true),
                    Production = table.Column<string>(nullable: true),
                    StartDate = table.Column<int>(nullable: true),
                    WebSiteLink = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StolonsUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
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
                    FamillyName = table.Column<string>(nullable: false),
                    Image = table.Column<string>(nullable: true),
                    TypeName = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProductFamillys", x => x.FamillyName);
                    table.ForeignKey(
                        name: "FK_ProductFamillys_ProductTypes_TypeName",
                        column: x => x.TypeName,
                        principalTable: "ProductTypes",
                        principalColumn: "Name",
                        onDelete: ReferentialAction.Restrict);
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
                    UserId = table.Column<int>(nullable: true),
                    UserName = table.Column<string>(maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_StolonsUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "StolonsUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConsumerBills",
                columns: table => new
                {
                    BillNumber = table.Column<string>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    ConsumerId = table.Column<int>(nullable: true),
                    EditionDate = table.Column<DateTime>(nullable: false),
                    ProducerId = table.Column<int>(nullable: true),
                    State = table.Column<int>(nullable: false),
                    UserId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConsumerBills", x => x.BillNumber);
                    table.ForeignKey(
                        name: "FK_ConsumerBills_StolonsUsers_ConsumerId",
                        column: x => x.ConsumerId,
                        principalTable: "StolonsUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsumerBills_StolonsUsers_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "StolonsUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConsumerBills_StolonsUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "StolonsUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "News",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    DateOfPublication = table.Column<DateTime>(nullable: false),
                    ImageLink = table.Column<string>(nullable: true),
                    Message = table.Column<string>(nullable: false),
                    Title = table.Column<string>(nullable: false),
                    UserForeignKey = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_News", x => x.Id);
                    table.ForeignKey(
                        name: "FK_News_StolonsUsers_UserForeignKey",
                        column: x => x.UserForeignKey,
                        principalTable: "StolonsUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ProducerBills",
                columns: table => new
                {
                    BillNumber = table.Column<string>(nullable: false),
                    Amount = table.Column<double>(nullable: false),
                    EditionDate = table.Column<DateTime>(nullable: false),
                    Fee = table.Column<int>(nullable: false),
                    ProducerId = table.Column<int>(nullable: true),
                    State = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProducerBills", x => x.BillNumber);
                    table.ForeignKey(
                        name: "FK_ProducerBills_StolonsUsers_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "StolonsUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TempsWeekBaskets",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ConsumerId = table.Column<int>(nullable: true),
                    Validated = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TempsWeekBaskets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TempsWeekBaskets_StolonsUsers_ConsumerId",
                        column: x => x.ConsumerId,
                        principalTable: "StolonsUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ValidatedWeekBaskets",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ConsumerId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ValidatedWeekBaskets", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ValidatedWeekBaskets_StolonsUsers_ConsumerId",
                        column: x => x.ConsumerId,
                        principalTable: "StolonsUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    AverageQuantity = table.Column<int>(nullable: false),
                    DLC = table.Column<DateTime>(nullable: false),
                    Description = table.Column<string>(nullable: true),
                    FamillyName = table.Column<string>(nullable: true),
                    LabelsSerialized = table.Column<string>(nullable: true),
                    Name = table.Column<string>(nullable: false),
                    PicturesSerialized = table.Column<string>(nullable: true),
                    Price = table.Column<float>(nullable: false),
                    ProducerId = table.Column<int>(nullable: true),
                    ProductUnit = table.Column<int>(nullable: false),
                    QuantityStep = table.Column<int>(nullable: false),
                    RemainingStock = table.Column<decimal>(nullable: false),
                    State = table.Column<int>(nullable: false),
                    Type = table.Column<int>(nullable: false),
                    UnitPrice = table.Column<float>(nullable: false),
                    WeekStock = table.Column<decimal>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_ProductFamillys_FamillyName",
                        column: x => x.FamillyName,
                        principalTable: "ProductFamillys",
                        principalColumn: "FamillyName",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Products_StolonsUsers_ProducerId",
                        column: x => x.ProducerId,
                        principalTable: "StolonsUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Autoincrement", true),
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
                name: "BillEntrys",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    ProductId = table.Column<Guid>(nullable: false),
                    Quantity = table.Column<int>(nullable: false),
                    TempWeekBasketId = table.Column<Guid>(nullable: true),
                    ValidatedWeekBasketId = table.Column<Guid>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillEntrys", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BillEntrys_Products_ProductId",
                        column: x => x.ProductId,
                        principalTable: "Products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

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
                name: "IX_AspNetUserRoles_UserId",
                table: "AspNetUserRoles",
                column: "UserId");

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
                name: "IX_BillEntrys_ProductId",
                table: "BillEntrys",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_BillEntrys_TempWeekBasketId",
                table: "BillEntrys",
                column: "TempWeekBasketId");

            migrationBuilder.CreateIndex(
                name: "IX_BillEntrys_ValidatedWeekBasketId",
                table: "BillEntrys",
                column: "ValidatedWeekBasketId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerBills_ConsumerId",
                table: "ConsumerBills",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerBills_ProducerId",
                table: "ConsumerBills",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_ConsumerBills_UserId",
                table: "ConsumerBills",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_News_UserForeignKey",
                table: "News",
                column: "UserForeignKey");

            migrationBuilder.CreateIndex(
                name: "IX_ProducerBills_ProducerId",
                table: "ProducerBills",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_Products_FamillyName",
                table: "Products",
                column: "FamillyName");

            migrationBuilder.CreateIndex(
                name: "IX_Products_ProducerId",
                table: "Products",
                column: "ProducerId");

            migrationBuilder.CreateIndex(
                name: "IX_ProductFamillys_TypeName",
                table: "ProductFamillys",
                column: "TypeName");

            migrationBuilder.CreateIndex(
                name: "IX_TempsWeekBaskets_ConsumerId",
                table: "TempsWeekBaskets",
                column: "ConsumerId");

            migrationBuilder.CreateIndex(
                name: "IX_ValidatedWeekBaskets_ConsumerId",
                table: "ValidatedWeekBaskets",
                column: "ConsumerId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
                name: "ApplicationConfig");

            migrationBuilder.DropTable(
                name: "BillEntrys");

            migrationBuilder.DropTable(
                name: "ConsumerBills");

            migrationBuilder.DropTable(
                name: "News");

            migrationBuilder.DropTable(
                name: "ProducerBills");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.DropTable(
                name: "TempsWeekBaskets");

            migrationBuilder.DropTable(
                name: "ValidatedWeekBaskets");

            migrationBuilder.DropTable(
                name: "ProductFamillys");

            migrationBuilder.DropTable(
                name: "StolonsUsers");

            migrationBuilder.DropTable(
                name: "ProductTypes");
        }
    }
}

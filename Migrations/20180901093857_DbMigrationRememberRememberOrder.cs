using Microsoft.EntityFrameworkCore.Migrations;
using System;
using System.Collections.Generic;

namespace Stolons.Migrations
{
    public partial class DbMigrationRememberRememberOrder : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OrderRememberHour",
                table: "Stolons",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ReceivedOrderRemember",
                table: "Adherents",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderRememberHour",
                table: "Stolons");

            migrationBuilder.DropColumn(
                name: "ReceivedOrderRemember",
                table: "Adherents");
        }
    }
}

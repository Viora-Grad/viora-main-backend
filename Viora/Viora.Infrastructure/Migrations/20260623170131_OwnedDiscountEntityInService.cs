using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OwnedDiscountEntityInService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountEndDateUtc",
                table: "Service",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "DiscountPercentage",
                table: "Service",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DiscountReason",
                table: "Service",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DiscountStartDateUtc",
                table: "Service",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DiscountEndDateUtc",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "DiscountReason",
                table: "Service");

            migrationBuilder.DropColumn(
                name: "DiscountStartDateUtc",
                table: "Service");
        }
    }
}

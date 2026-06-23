using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMoneyToPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "SubscriptionOrder");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "LimitedFeatureAddons");

            migrationBuilder.DropColumn(
                name: "TotalPrice",
                table: "AddonOrders");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPriceAmount",
                table: "SubscriptionOrder",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TotalPriceCurrency",
                table: "SubscriptionOrder",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAmount",
                table: "Plans",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PriceCurrency",
                table: "Plans",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "PriceAmount",
                table: "LimitedFeatureAddons",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "PriceCurrency",
                table: "LimitedFeatureAddons",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "TotalPriceAmount",
                table: "AddonOrders",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "TotalPriceCurrency",
                table: "AddonOrders",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TotalPriceAmount",
                table: "SubscriptionOrder");

            migrationBuilder.DropColumn(
                name: "TotalPriceCurrency",
                table: "SubscriptionOrder");

            migrationBuilder.DropColumn(
                name: "PriceAmount",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "PriceCurrency",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "PriceAmount",
                table: "LimitedFeatureAddons");

            migrationBuilder.DropColumn(
                name: "PriceCurrency",
                table: "LimitedFeatureAddons");

            migrationBuilder.DropColumn(
                name: "TotalPriceAmount",
                table: "AddonOrders");

            migrationBuilder.DropColumn(
                name: "TotalPriceCurrency",
                table: "AddonOrders");

            migrationBuilder.AddColumn<double>(
                name: "TotalPrice",
                table: "SubscriptionOrder",
                type: "float(18)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "Plans",
                type: "float(18)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "Price",
                table: "LimitedFeatureAddons",
                type: "float(18)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalPrice",
                table: "AddonOrders",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationSchemaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ProposedServiceType",
                table: "OrganizationApplications");

            migrationBuilder.AlterColumn<DateTime>(
                name: "JoinedOnUtc",
                table: "Organizations",
                type: "datetime2",
                nullable: false,
                oldClrType: typeof(DateOnly),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "About",
                table: "Organizations",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ServicesProvided",
                table: "Organizations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "About",
                table: "OrganizationApplications",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProposedServicesType",
                table: "OrganizationApplications",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "About",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ServicesProvided",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "About",
                table: "OrganizationApplications");

            migrationBuilder.DropColumn(
                name: "ProposedServicesType",
                table: "OrganizationApplications");

            migrationBuilder.AlterColumn<DateOnly>(
                name: "JoinedOnUtc",
                table: "Organizations",
                type: "date",
                nullable: false,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "Organizations",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ProposedServiceType",
                table: "OrganizationApplications",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");
        }
    }
}

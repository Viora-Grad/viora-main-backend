using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class CustomerPicandConversions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Contact");

            migrationBuilder.AddColumn<string>(
                name: "Emails",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumbers",
                table: "Customers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ProfilePicId",
                table: "Customers",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_ProfilePicId",
                table: "Customers",
                column: "ProfilePicId",
                unique: true,
                filter: "[ProfilePicId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_MediaFiles_ProfilePicId",
                table: "Customers",
                column: "ProfilePicId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_MediaFiles_ProfilePicId",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_ProfilePicId",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "Emails",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "PhoneNumbers",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "ProfilePicId",
                table: "Customers");

            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => new { x.CustomerId, x.Id });
                    table.ForeignKey(
                        name: "FK_Contact_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}

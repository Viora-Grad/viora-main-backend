using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class unknwonChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Organizations_OrganizationId",
                table: "Staff");

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Organizations_OrganizationId",
                table: "Staff",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Staff_Organizations_OrganizationId",
                table: "Staff");

            migrationBuilder.AddForeignKey(
                name: "FK_Staff_Organizations_OrganizationId",
                table: "Staff",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

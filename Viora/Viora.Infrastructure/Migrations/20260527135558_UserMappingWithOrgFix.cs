using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UserMappingWithOrgFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationApplications_Owners_OwnerId",
                table: "OrganizationApplications");

            // migrationBuilder.InsertData(
            //     table: "Role",
            //     columns: new[] { "Id", "Name" },
            //     values: new object[] { 2, "Owner" });

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationApplications_Users_OwnerId",
                table: "OrganizationApplications",
                column: "OwnerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationApplications_Users_OwnerId",
                table: "OrganizationApplications");

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationApplications_Owners_OwnerId",
                table: "OrganizationApplications",
                column: "OwnerId",
                principalTable: "Owners",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

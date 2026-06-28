using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MappedRelationLegalPaperToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApplicationId",
                table: "LegalPapers",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_LegalPapers_ApplicationId",
                table: "LegalPapers",
                column: "ApplicationId");

            migrationBuilder.AddForeignKey(
                name: "FK_LegalPapers_OrganizationApplications_ApplicationId",
                table: "LegalPapers",
                column: "ApplicationId",
                principalTable: "OrganizationApplications",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LegalPapers_OrganizationApplications_ApplicationId",
                table: "LegalPapers");

            migrationBuilder.DropIndex(
                name: "IX_LegalPapers_ApplicationId",
                table: "LegalPapers");

            migrationBuilder.DropColumn(
                name: "ApplicationId",
                table: "LegalPapers");
        }
    }
}

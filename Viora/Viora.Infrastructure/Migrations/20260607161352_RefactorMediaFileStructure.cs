using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorMediaFileStructure : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceGallery_MediaFiles_MediaFileId",
                table: "ServiceGallery");

            migrationBuilder.AddColumn<Guid>(
                name: "OrganizationId",
                table: "MediaFiles",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "BranchGallery",
                columns: table => new
                {
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MediaFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BranchGallery", x => new { x.BranchId, x.MediaFileId });
                    table.ForeignKey(
                        name: "FK_BranchGallery_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BranchGallery_MediaFiles_MediaFileId",
                        column: x => x.MediaFileId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_OrganizationId",
                table: "MediaFiles",
                column: "OrganizationId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchGallery_BranchId",
                table: "BranchGallery",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_BranchGallery_MediaFileId",
                table: "BranchGallery",
                column: "MediaFileId");

            migrationBuilder.AddForeignKey(
                name: "FK_MediaFiles_Organizations_OrganizationId",
                table: "MediaFiles",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceGallery_MediaFiles_MediaFileId",
                table: "ServiceGallery",
                column: "MediaFileId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MediaFiles_Organizations_OrganizationId",
                table: "MediaFiles");

            migrationBuilder.DropForeignKey(
                name: "FK_ServiceGallery_MediaFiles_MediaFileId",
                table: "ServiceGallery");

            migrationBuilder.DropTable(
                name: "BranchGallery");

            migrationBuilder.DropIndex(
                name: "IX_MediaFiles_OrganizationId",
                table: "MediaFiles");

            migrationBuilder.DropColumn(
                name: "OrganizationId",
                table: "MediaFiles");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceGallery_MediaFiles_MediaFileId",
                table: "ServiceGallery",
                column: "MediaFileId",
                principalTable: "MediaFiles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}

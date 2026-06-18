using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorPlan : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanFeatures_Features_FeatureId",
                table: "PlanFeatures");

            migrationBuilder.DropForeignKey(
                name: "FK_PlanFeatures_LimitedFeatures_LimitedFeatureId",
                table: "PlanFeatures");

            migrationBuilder.DropIndex(
                name: "IX_PlanFeatures_LimitedFeatureId",
                table: "PlanFeatures");

            migrationBuilder.DropColumn(
                name: "LimitedFeatureId",
                table: "PlanFeatures");

            migrationBuilder.DropColumn(
                name: "Limit",
                table: "LimitedFeatures");

            migrationBuilder.AlterColumn<Guid>(
                name: "FeatureId",
                table: "PlanFeatures",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier",
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "PlanLimitedFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LimitedFeatureId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LimitValue = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanLimitedFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanLimitedFeatures_LimitedFeatures_LimitedFeatureId",
                        column: x => x.LimitedFeatureId,
                        principalTable: "LimitedFeatures",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PlanLimitedFeatures_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PlanLimitedFeatures_LimitedFeatureId",
                table: "PlanLimitedFeatures",
                column: "LimitedFeatureId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanLimitedFeatures_PlanId",
                table: "PlanLimitedFeatures",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanFeatures_Features_FeatureId",
                table: "PlanFeatures",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PlanFeatures_Features_FeatureId",
                table: "PlanFeatures");

            migrationBuilder.DropTable(
                name: "PlanLimitedFeatures");

            migrationBuilder.AlterColumn<Guid>(
                name: "FeatureId",
                table: "PlanFeatures",
                type: "uniqueidentifier",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier");

            migrationBuilder.AddColumn<Guid>(
                name: "LimitedFeatureId",
                table: "PlanFeatures",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<long>(
                name: "Limit",
                table: "LimitedFeatures",
                type: "bigint",
                nullable: false,
                defaultValue: 0L);

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatures_LimitedFeatureId",
                table: "PlanFeatures",
                column: "LimitedFeatureId");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanFeatures_Features_FeatureId",
                table: "PlanFeatures",
                column: "FeatureId",
                principalTable: "Features",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_PlanFeatures_LimitedFeatures_LimitedFeatureId",
                table: "PlanFeatures",
                column: "LimitedFeatureId",
                principalTable: "LimitedFeatures",
                principalColumn: "Id");
        }
    }
}

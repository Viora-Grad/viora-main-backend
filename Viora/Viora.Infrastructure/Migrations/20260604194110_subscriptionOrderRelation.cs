using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class subscriptionOrderRelation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionOrder_PlanId",
                table: "SubscriptionOrder",
                column: "PlanId");

            migrationBuilder.AddForeignKey(
                name: "FK_SubscriptionOrder_Plans_PlanId",
                table: "SubscriptionOrder",
                column: "PlanId",
                principalTable: "Plans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_SubscriptionOrder_Plans_PlanId",
                table: "SubscriptionOrder");

            migrationBuilder.DropIndex(
                name: "IX_SubscriptionOrder_PlanId",
                table: "SubscriptionOrder");
        }
    }
}

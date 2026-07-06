using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    // Hand-authored (design-time EF tooling was blocked by local Application Control policy). The
    // [DbContext]/[Migration] attributes let the runtime migrator discover and apply this via Up().
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260705120000_MarketingImagePost")]
    public partial class MarketingImagePost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LatestImageUrl",
                table: "MarketingChatSessions",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LatestImageUrl",
                table: "MarketingChatSessions");
        }
    }
}

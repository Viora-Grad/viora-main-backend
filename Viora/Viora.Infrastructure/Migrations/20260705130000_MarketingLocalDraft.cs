using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    // Hand-authored (design-time EF tooling was blocked by local Application Control policy). Adds the local
    // draft columns used by the archive->publish (one-shot create) flow.
    [DbContext(typeof(ApplicationDbContext))]
    [Migration("20260705130000_MarketingLocalDraft")]
    public partial class MarketingLocalDraft : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PostMessage",
                table: "MarketingChatSessions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostLink",
                table: "MarketingChatSessions",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PostMessage",
                table: "MarketingChatSessions");

            migrationBuilder.DropColumn(
                name: "PostLink",
                table: "MarketingChatSessions");
        }
    }
}

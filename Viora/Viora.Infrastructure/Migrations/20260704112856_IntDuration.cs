using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class IntDuration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDuration",
                table: "Appointments");

            migrationBuilder.AddColumn<int>(
                name: "EstimatedDurationMinutes",
                table: "Appointments",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EstimatedDurationMinutes",
                table: "Appointments");

            migrationBuilder.AddColumn<double>(
                name: "EstimatedDuration",
                table: "Appointments",
                type: "float",
                nullable: false,
                defaultValue: 0.0);
        }
    }
}

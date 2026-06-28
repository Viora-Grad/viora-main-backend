using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FormModuleModification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FormSubmissions_AppointmentId",
                table: "FormSubmissions");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_AppointmentId",
                table: "FormSubmissions",
                column: "AppointmentId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_FormSubmissions_AppointmentId",
                table: "FormSubmissions");

            migrationBuilder.CreateIndex(
                name: "IX_FormSubmissions_AppointmentId",
                table: "FormSubmissions",
                column: "AppointmentId");
        }
    }
}

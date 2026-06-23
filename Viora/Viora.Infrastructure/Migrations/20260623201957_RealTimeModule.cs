using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RealTimeModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScheduleDelays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AppointmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DelayDuration = table.Column<TimeOnly>(type: "time", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    OccurrenceTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Initiator = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleDelays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleDelays_Appointments_AppointmentId",
                        column: x => x.AppointmentId,
                        principalTable: "Appointments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Schedules",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DayOfWeek = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Schedules", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Staff",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Staff", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Staff_Branch_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branch",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Shifts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ScheduleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StaffId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartTime = table.Column<TimeOnly>(type: "time", nullable: false),
                    EndTime = table.Column<TimeOnly>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Shifts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Shifts_Schedules_ScheduleId",
                        column: x => x.ScheduleId,
                        principalTable: "Schedules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleCancellations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ShiftId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CancellationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleCancellations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleCancellations_Shifts_ShiftId",
                        column: x => x.ShiftId,
                        principalTable: "Shifts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleCancellations_CancellationDate",
                table: "ScheduleCancellations",
                column: "CancellationDate");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleCancellations_ShiftId",
                table: "ScheduleCancellations",
                column: "ShiftId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDelays_AppointmentId",
                table: "ScheduleDelays",
                column: "AppointmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleDelays_OccurrenceTime",
                table: "ScheduleDelays",
                column: "OccurrenceTime");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_BranchId_DayOfWeek",
                table: "Schedules",
                columns: new[] { "BranchId", "DayOfWeek" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_ScheduleId_StaffId",
                table: "Shifts",
                columns: new[] { "ScheduleId", "StaffId" });

            migrationBuilder.CreateIndex(
                name: "IX_Shifts_StaffId",
                table: "Shifts",
                column: "StaffId");

            migrationBuilder.CreateIndex(
                name: "IX_Staff_BranchId",
                table: "Staff",
                column: "BranchId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduleCancellations");

            migrationBuilder.DropTable(
                name: "ScheduleDelays");

            migrationBuilder.DropTable(
                name: "Staff");

            migrationBuilder.DropTable(
                name: "Shifts");

            migrationBuilder.DropTable(
                name: "Schedules");
        }
    }
}

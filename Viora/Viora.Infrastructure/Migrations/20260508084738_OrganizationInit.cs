using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrganizationInit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeatureUsages_Organization_OrganizationId",
                table: "FeatureUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationVisits_Organization_OrganizationId",
                table: "OrganizationVisits");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Organization_OrganizationId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "Organization");

            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    IsoAlphaThree = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nationality = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaFiles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SizeInBytes = table.Column<long>(type: "bigint", nullable: false),
                    Key = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledDomainEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EventType = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Payload = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScheduledFor = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    ProcessedOn = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    IsCancelled = table.Column<bool>(type: "bit", nullable: false),
                    AttemptCount = table.Column<int>(type: "int", nullable: false),
                    Error = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledDomainEvents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OrganizationApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProposedServiceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubmittedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReferralSource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RejectedBy = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpiryDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ApplicationLetter = table.Column<string>(type: "nvarchar(max)", maxLength: 5000, nullable: false),
                    BillingEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    ProposedName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ServiceDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SupportEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrganizationApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OrganizationApplications_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OrganizationApplications_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "LegalPapers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttachmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApprovedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubmissionDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalPapers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LegalPapers_MediaFiles_AttachmentId",
                        column: x => x.AttachmentId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Organizations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LogoId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    JoinedOnUtc = table.Column<DateOnly>(type: "date", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    ReferralSource = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    BillingEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false),
                    RatingAverage = table.Column<decimal>(type: "decimal(3,1)", nullable: false),
                    RatingCount = table.Column<int>(type: "int", nullable: false),
                    ServiceDescription = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SupportEmail = table.Column<string>(type: "nvarchar(320)", maxLength: 320, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organizations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Organizations_MediaFiles_LogoId",
                        column: x => x.LogoId,
                        principalTable: "MediaFiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Suspensions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrganizationId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    OwnerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuspendedById = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Source = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SuspensionDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledDeletionDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OrganizationName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Suspensions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Suspensions_Organizations_OrganizationId",
                        column: x => x.OrganizationId,
                        principalTable: "Organizations",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Suspensions_Owners_OwnerId",
                        column: x => x.OwnerId,
                        principalTable: "Owners",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LegalPapers_AttachmentId",
                table: "LegalPapers",
                column: "AttachmentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LegalPapers_Status",
                table: "LegalPapers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_Key",
                table: "MediaFiles",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MediaFiles_UploadedAtUtc",
                table: "MediaFiles",
                column: "UploadedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationApplications_CountryId",
                table: "OrganizationApplications",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationApplications_OwnerId",
                table: "OrganizationApplications",
                column: "OwnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OrganizationApplications_Status",
                table: "OrganizationApplications",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_CountryId",
                table: "Organizations",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_LogoId",
                table: "Organizations",
                column: "LogoId");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Name",
                table: "Organizations",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_OwnerId",
                table: "Organizations",
                column: "OwnerId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Organizations_Status",
                table: "Organizations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledDomainEvents_ProcessedOn_ScheduledFor",
                table: "ScheduledDomainEvents",
                columns: new[] { "ProcessedOn", "ScheduledFor" },
                filter: "[ProcessedOn] IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Suspensions_OrganizationId",
                table: "Suspensions",
                column: "OrganizationId",
                unique: true,
                filter: "[OrganizationId] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Suspensions_OwnerId",
                table: "Suspensions",
                column: "OwnerId");

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureUsages_Organizations_OrganizationId",
                table: "FeatureUsages",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationVisits_Organizations_OrganizationId",
                table: "OrganizationVisits",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Organizations_OrganizationId",
                table: "Subscriptions",
                column: "OrganizationId",
                principalTable: "Organizations",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FeatureUsages_Organizations_OrganizationId",
                table: "FeatureUsages");

            migrationBuilder.DropForeignKey(
                name: "FK_OrganizationVisits_Organizations_OrganizationId",
                table: "OrganizationVisits");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Organizations_OrganizationId",
                table: "Subscriptions");

            migrationBuilder.DropTable(
                name: "LegalPapers");

            migrationBuilder.DropTable(
                name: "OrganizationApplications");

            migrationBuilder.DropTable(
                name: "ScheduledDomainEvents");

            migrationBuilder.DropTable(
                name: "Suspensions");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Organizations");

            migrationBuilder.DropTable(
                name: "MediaFiles");

            migrationBuilder.CreateTable(
                name: "Organization",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Organization", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_FeatureUsages_Organization_OrganizationId",
                table: "FeatureUsages",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrganizationVisits_Organization_OrganizationId",
                table: "OrganizationVisits",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Organization_OrganizationId",
                table: "Subscriptions",
                column: "OrganizationId",
                principalTable: "Organization",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

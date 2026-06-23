using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropOrphanedContactTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Customer contacts were refactored from the owned "Contact" table into
            // semicolon-separated PhoneNumbers/Emails columns on the Customers table.
            // The Contact table is stale in the model snapshot and may not exist in every
            // database, so guard the whole step on its existence: where it does exist, copy
            // any rows into the new inline columns (read-side converter de-duplicates) before
            // dropping it; where it doesn't, this is a no-op.
            migrationBuilder.Sql(@"
                IF OBJECT_ID(N'[Contact]', N'U') IS NOT NULL
                BEGIN
                    UPDATE c
                    SET c.PhoneNumbers = agg.PhoneNumbers,
                        c.Emails       = agg.Emails
                    FROM Customers c
                    INNER JOIN (
                        SELECT CustomerId,
                               STRING_AGG(PhoneNumber, ';') AS PhoneNumbers,
                               STRING_AGG(Email, ';')       AS Emails
                        FROM Contact
                        GROUP BY CustomerId
                    ) agg ON agg.CustomerId = c.Id;

                    DROP TABLE [Contact];
                END");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Contact",
                columns: table => new
                {
                    CustomerId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contact", x => new { x.CustomerId, x.Id });
                    table.ForeignKey(
                        name: "FK_Contact_Customers_CustomerId",
                        column: x => x.CustomerId,
                        principalTable: "Customers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }
    }
}

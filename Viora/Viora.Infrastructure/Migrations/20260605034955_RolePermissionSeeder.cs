using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Viora.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RolePermissionSeeder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // migrationBuilder.InsertData(
            //     table: "Role",
            //     columns: new[] { "Id", "Name" },
            //     values: new object[,]
            //     {
            //         { 2, "Owner" },
            //         { 3, "Admin" },
            //         { 4, "Customer" }
            //     });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 2, "users:write" },
                    { 10, "roles:read" },
                    { 11, "roles:write" },
                    { 20, "plans:read" },
                    { 21, "plans:write" },
                    { 30, "subscriptions:manage" },
                    { 40, "features:read" },
                    { 41, "features:write" }
                });

            // migrationBuilder.InsertData(
            //     table: "RolePermissions",
            //     columns: new[] { "PermissionId", "RoleId" },
            //     values: new object[,]
            //     {
            //         { 1, 2 },
            //         { 10, 2 },
            //         { 20, 2 },
            //         { 30, 2 },
            //         { 40, 2 },
            //         { 1, 3 },
            //         { 2, 3 },
            //         { 10, 3 },
            //         { 11, 3 },
            //         { 20, 3 },
            //         { 21, 3 },
            //         { 30, 3 },
            //         { 40, 3 },
            //         { 41, 3 }
            //     });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 20, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 30, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 40, 2 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 1, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 2, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 10, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 11, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 20, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 21, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 30, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 40, 3 });

            migrationBuilder.DeleteData(
                table: "RolePermissions",
                keyColumns: new[] { "PermissionId", "RoleId" },
                keyValues: new object[] { 41, 3 });

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Role",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "Id",
                keyValue: 41);
        }
    }
}

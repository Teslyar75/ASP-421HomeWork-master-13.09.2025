using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_421.Migrations
{
    /// <inheritdoc />
    public partial class InitialCorrection : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserAccesses",
                keyColumn: "Id",
                keyValue: new Guid("db1326be-b0cc-4372-beb8-14c60bd822d1"));

            migrationBuilder.InsertData(
                table: "UserAccesses",
                columns: new[] { "Id", "Dk", "Login", "RoleId", "Salt", "UserId" },
                values: new object[] { new Guid("2570a0d2-fab2-4de0-8efc-e2bd28de2502"), "1678112717E7AF0947F6", "Admin", "Admin", "4FA5D20B-E546-4818-9381-B4BD9F327F4E", new Guid("53759101-7de4-4e04-833a-884752290fa0") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("53759101-7de4-4e04-833a-884752290fa0"),
                columns: new[] { "Birthdate", "RegisteredAt" },
                values: new object[] { new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "UserAccesses",
                keyColumn: "Id",
                keyValue: new Guid("2570a0d2-fab2-4de0-8efc-e2bd28de2502"));

            migrationBuilder.InsertData(
                table: "UserAccesses",
                columns: new[] { "Id", "Dk", "Login", "RoleId", "Salt", "UserId" },
                values: new object[] { new Guid("db1326be-b0cc-4372-beb8-14c60bd822d1"), "1678112717E7AF0947F6", "Admin", "Admin", "4FA5D20B-E546-4818-9381-B4BD9F327F4E", new Guid("53759101-7de4-4e04-833a-884752290fa0") });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: new Guid("53759101-7de4-4e04-833a-884752290fa0"),
                columns: new[] { "Birthdate", "RegisteredAt" },
                values: new object[] { new DateTime(2025, 8, 30, 11, 36, 42, 763, DateTimeKind.Local).AddTicks(3795), new DateTime(2025, 8, 30, 11, 36, 42, 766, DateTimeKind.Local).AddTicks(8764) });
        }
    }
}

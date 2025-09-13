using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ASP_421.Migrations
{
    /// <inheritdoc />
    public partial class AddVisitsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Visits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VisitTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RequestPath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    UserLogin = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ConfirmationCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsConfirmed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ConfirmedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Visits", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Visits_ConfirmationCode",
                table: "Visits",
                column: "ConfirmationCode");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_IsConfirmed",
                table: "Visits",
                column: "IsConfirmed");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_RequestPath",
                table: "Visits",
                column: "RequestPath");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_UserLogin",
                table: "Visits",
                column: "UserLogin");

            migrationBuilder.CreateIndex(
                name: "IX_Visits_VisitTime",
                table: "Visits",
                column: "VisitTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Visits");
        }
    }
}

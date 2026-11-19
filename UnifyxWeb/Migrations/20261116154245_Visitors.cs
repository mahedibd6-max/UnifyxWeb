using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifyxWeb.Migrations
{
    /// <inheritdoc />
    public partial class Visitors : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "VisitDays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PageViews = table.Column<int>(type: "int", nullable: false),
                    Visitors = table.Column<int>(type: "int", nullable: false),
                    NewVisitors = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitDays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "VisitPages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Path = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Views = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VisitPages", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_VisitDays_Date",
                table: "VisitDays",
                column: "Date",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VisitPages_Date_Path",
                table: "VisitPages",
                columns: new[] { "Date", "Path" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "VisitDays");

            migrationBuilder.DropTable(
                name: "VisitPages");
        }
    }
}

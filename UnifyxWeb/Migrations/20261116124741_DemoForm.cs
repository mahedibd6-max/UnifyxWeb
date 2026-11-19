using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifyxWeb.Migrations
{
    /// <inheritdoc />
    public partial class DemoForm : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AdminNote",
                table: "DemoRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "DemoRequests",
                type: "nvarchar(2)",
                maxLength: 2,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Modules",
                table: "DemoRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AdminNote",
                table: "DemoRequests");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "DemoRequests");

            migrationBuilder.DropColumn(
                name: "Modules",
                table: "DemoRequests");
        }
    }
}

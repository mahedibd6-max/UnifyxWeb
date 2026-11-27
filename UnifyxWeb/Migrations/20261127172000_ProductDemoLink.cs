using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifyxWeb.Migrations
{
    /// <inheritdoc />
    public partial class ProductDemoLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DemoUrl",
                table: "Products",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DemoUrl",
                table: "Products");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifyxWeb.Migrations
{
    /// <inheritdoc />
    public partial class ProductSeo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PriceFrom",
                table: "Products",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescriptionBn",
                table: "Products",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoDescriptionEn",
                table: "Products",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitleBn",
                table: "Products",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SeoTitleEn",
                table: "Products",
                type: "nvarchar(70)",
                maxLength: 70,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PriceFrom",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SeoDescriptionBn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SeoDescriptionEn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SeoTitleBn",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SeoTitleEn",
                table: "Products");
        }
    }
}

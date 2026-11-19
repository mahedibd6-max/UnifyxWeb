using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifyxWeb.Migrations
{
    /// <inheritdoc />
    public partial class Products : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Slug = table.Column<string>(type: "nvarchar(60)", maxLength: 60, nullable: false),
                    NameEn = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NameBn = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    Icon = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    TaglineEn = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    TaglineBn = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    DescriptionEn = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    DescriptionBn = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    FeaturesEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    FeaturesBn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    AppUrl = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UseHomePage = table.Column<bool>(type: "bit", nullable: false),
                    ComingSoon = table.Column<bool>(type: "bit", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_Slug",
                table: "Products",
                column: "Slug",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");
        }
    }
}

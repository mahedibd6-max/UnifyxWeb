using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifyxWeb.Migrations
{
    /// <inheritdoc />
    public partial class HomeNewsFaq : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "FaqItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuestionEn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    QuestionBn = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    AnswerEn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    AnswerBn = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FaqItems", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeContents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EyebrowEn = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EyebrowBn = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TitleEn = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    TitleBn = table.Column<string>(type: "nvarchar(160)", maxLength: 160, nullable: true),
                    LeadEn = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    LeadBn = table.Column<string>(type: "nvarchar(600)", maxLength: 600, nullable: true),
                    HeroImagePath = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeContents", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "HomeStats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Value = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    LabelEn = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    LabelBn = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HomeStats", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "FaqItems");

            migrationBuilder.DropTable(
                name: "HomeContents");

            migrationBuilder.DropTable(
                name: "HomeStats");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnifyxWeb.Migrations
{
    /// <inheritdoc />
    public partial class HomeModules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ModuleItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Icon = table.Column<string>(type: "nvarchar(8)", maxLength: 8, nullable: true),
                    NameEn = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    NameBn = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    TextEn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    TextBn = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PointsEn = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    PointsBn = table.Column<string>(type: "nvarchar(800)", maxLength: 800, nullable: true),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleItems", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ModuleItems");
        }
    }
}

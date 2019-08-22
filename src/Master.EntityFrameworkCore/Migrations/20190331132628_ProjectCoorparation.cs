using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class ProjectCoorparation : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Coorparation",
                table: "Projects",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coorparation",
                table: "Projects");
        }
    }
}

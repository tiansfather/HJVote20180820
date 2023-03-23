using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class Project_IsPreRejected : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsPreRejected",
                table: "Projects",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsPreRejected",
                table: "Projects");
        }
    }
}

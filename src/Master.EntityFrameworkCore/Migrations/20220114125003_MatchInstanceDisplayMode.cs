using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class MatchInstanceDisplayMode : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MatchInstanceDisplayMode",
                table: "MatchInstances",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchInstanceDisplayMode",
                table: "MatchInstances");
        }
    }
}

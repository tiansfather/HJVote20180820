using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class MatchInstanceType : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MatchInstanceType",
                table: "MatchInstances",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MatchInstanceType",
                table: "MatchInstances");
        }
    }
}

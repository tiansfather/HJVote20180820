using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class ShowScope : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayScope",
                table: "MatchInstances",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "DisplayName",
                table: "Matches",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDisplay",
                table: "Matches",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayScope",
                table: "MatchInstances");

            migrationBuilder.DropColumn(
                name: "DisplayName",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "IsDisplay",
                table: "Matches");
        }
    }
}

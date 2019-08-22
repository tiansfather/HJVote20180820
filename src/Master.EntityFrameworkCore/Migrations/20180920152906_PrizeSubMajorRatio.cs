using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class PrizeSubMajorRatio : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "Ratio",
                table: "PrizeSubMajors",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Ratio",
                table: "PrizeSubMajors");
        }
    }
}

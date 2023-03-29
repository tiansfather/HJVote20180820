using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class Project_Pre : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInInitialReview",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPreRejected",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ScorePre",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScorePre",
                table: "ProjectMajorInfos",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInInitialReview",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsPreRejected",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ScorePre",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ScorePre",
                table: "ProjectMajorInfos");
        }
    }
}
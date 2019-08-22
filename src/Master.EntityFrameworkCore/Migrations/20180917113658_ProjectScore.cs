using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class ProjectScore : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsInFinalReview",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RankFinal",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RankInitial",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScoreFinal",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScoreInitial",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScoreFinal",
                table: "ProjectMajorInfos",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScoreInitial",
                table: "ProjectMajorInfos",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsInFinalReview",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RankFinal",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RankInitial",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ScoreFinal",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ScoreInitial",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ScoreFinal",
                table: "ProjectMajorInfos");

            migrationBuilder.DropColumn(
                name: "ScoreInitial",
                table: "ProjectMajorInfos");
        }
    }
}

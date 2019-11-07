using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class CrossProject : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CrossProjectId",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsInChampionReview",
                table: "Projects",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RankChampion",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScoreChampion",
                table: "Projects",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Projects_CrossProjectId",
                table: "Projects",
                column: "CrossProjectId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Projects_CrossProjectId",
                table: "Projects",
                column: "CrossProjectId",
                principalTable: "Projects",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Projects_CrossProjectId",
                table: "Projects");

            migrationBuilder.DropIndex(
                name: "IX_Projects_CrossProjectId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "CrossProjectId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "IsInChampionReview",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RankChampion",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ScoreChampion",
                table: "Projects");
        }
    }
}

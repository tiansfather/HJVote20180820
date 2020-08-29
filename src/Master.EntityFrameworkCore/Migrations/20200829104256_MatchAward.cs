using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class MatchAward : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MatchAwardId",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RankManual",
                table: "Projects",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "ScoreManual",
                table: "Projects",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "MatchAwards",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    MatchId = table.Column<int>(nullable: false),
                    AwardName = table.Column<string>(nullable: true),
                    AwardRank = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatchAwards", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatchAwards_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Projects_MatchAwardId",
                table: "Projects",
                column: "MatchAwardId");

            migrationBuilder.CreateIndex(
                name: "IX_MatchAwards_MatchId",
                table: "MatchAwards",
                column: "MatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_MatchAwards_MatchAwardId",
                table: "Projects",
                column: "MatchAwardId",
                principalTable: "MatchAwards",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Projects_MatchAwards_MatchAwardId",
                table: "Projects");

            migrationBuilder.DropTable(
                name: "MatchAwards");

            migrationBuilder.DropIndex(
                name: "IX_Projects_MatchAwardId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "MatchAwardId",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "RankManual",
                table: "Projects");

            migrationBuilder.DropColumn(
                name: "ScoreManual",
                table: "Projects");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class Prize_Group : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PrizeGroupId",
                table: "Prizes",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "PrizeGroups",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    MatchId = table.Column<int>(nullable: true),
                    GroupName = table.Column<string>(nullable: true),
                    Remarks = table.Column<string>(nullable: true),
                    IsActive = table.Column<bool>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PrizeGroups", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PrizeGroups_Matches_MatchId",
                        column: x => x.MatchId,
                        principalTable: "Matches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Prizes_PrizeGroupId",
                table: "Prizes",
                column: "PrizeGroupId");

            migrationBuilder.CreateIndex(
                name: "IX_PrizeGroups_MatchId",
                table: "PrizeGroups",
                column: "MatchId");

            migrationBuilder.AddForeignKey(
                name: "FK_Prizes_PrizeGroups_PrizeGroupId",
                table: "Prizes",
                column: "PrizeGroupId",
                principalTable: "PrizeGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Prizes_PrizeGroups_PrizeGroupId",
                table: "Prizes");

            migrationBuilder.DropTable(
                name: "PrizeGroups");

            migrationBuilder.DropIndex(
                name: "IX_Prizes_PrizeGroupId",
                table: "Prizes");

            migrationBuilder.DropColumn(
                name: "PrizeGroupId",
                table: "Prizes");
        }
    }
}

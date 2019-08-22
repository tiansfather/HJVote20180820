using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class reviewRound : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ReviewStatus",
                table: "Reviews",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "StartTime",
                table: "Reviews",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ReviewRounds",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    ExtensionData = table.Column<string>(nullable: true),
                    ReviewId = table.Column<int>(nullable: false),
                    Round = table.Column<int>(nullable: false),
                    Turn = table.Column<int>(nullable: false),
                    TargetNumber = table.Column<int>(nullable: false),
                    ReviewMethod = table.Column<int>(nullable: false),
                    ReviewStatus = table.Column<int>(nullable: false),
                    SourceProjectIDs = table.Column<string>(nullable: true),
                    ResultProjectIDs = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReviewRounds", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ReviewRounds_Reviews_ReviewId",
                        column: x => x.ReviewId,
                        principalTable: "Reviews",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ReviewRounds_ReviewId",
                table: "ReviewRounds",
                column: "ReviewId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ReviewRounds");

            migrationBuilder.DropColumn(
                name: "ReviewStatus",
                table: "Reviews");

            migrationBuilder.DropColumn(
                name: "StartTime",
                table: "Reviews");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class review : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Reviews",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:ValueGenerationStrategy", SqlServerValueGenerationStrategy.IdentityColumn),
                    CreationTime = table.Column<DateTime>(nullable: false),
                    CreatorUserId = table.Column<long>(nullable: true),
                    ExtensionData = table.Column<string>(nullable: true),
                    MatchInstanceId = table.Column<int>(nullable: false),
                    MajorId = table.Column<int>(nullable: false),
                    SubMajorId = table.Column<int>(nullable: true),
                    ReviewMajorName = table.Column<string>(nullable: true),
                    ReviewName = table.Column<string>(nullable: true),
                    Remarks = table.Column<string>(nullable: true),
                    ReviewType = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reviews", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reviews_Majors_MajorId",
                        column: x => x.MajorId,
                        principalTable: "Majors",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Reviews_MatchInstances_MatchInstanceId",
                        column: x => x.MatchInstanceId,
                        principalTable: "MatchInstances",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MajorId",
                table: "Reviews",
                column: "MajorId");

            migrationBuilder.CreateIndex(
                name: "IX_Reviews_MatchInstanceId",
                table: "Reviews",
                column: "MatchInstanceId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Reviews");
        }
    }
}

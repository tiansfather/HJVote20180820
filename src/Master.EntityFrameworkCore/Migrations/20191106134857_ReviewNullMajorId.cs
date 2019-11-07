using Microsoft.EntityFrameworkCore.Migrations;

namespace Master.Migrations
{
    public partial class ReviewNullMajorId : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Majors_MajorId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "MajorId",
                table: "Reviews",
                nullable: true,
                oldClrType: typeof(int));

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Majors_MajorId",
                table: "Reviews",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Reviews_Majors_MajorId",
                table: "Reviews");

            migrationBuilder.AlterColumn<int>(
                name: "MajorId",
                table: "Reviews",
                nullable: false,
                oldClrType: typeof(int),
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Reviews_Majors_MajorId",
                table: "Reviews",
                column: "MajorId",
                principalTable: "Majors",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

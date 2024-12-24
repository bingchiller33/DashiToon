using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateKanaTransactionAddChapterref : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChapterId",
                table: "KanaTransaction",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_KanaTransaction_ChapterId",
                table: "KanaTransaction",
                column: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_KanaTransaction_Chapters_ChapterId",
                table: "KanaTransaction",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_KanaTransaction_Chapters_ChapterId",
                table: "KanaTransaction");

            migrationBuilder.DropIndex(
                name: "IX_KanaTransaction_ChapterId",
                table: "KanaTransaction");

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "KanaTransaction");
        }
    }
}

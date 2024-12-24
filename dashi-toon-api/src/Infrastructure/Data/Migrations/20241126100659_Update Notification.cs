using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateNotification : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ChapterId",
                table: "Notifications",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ChapterId",
                table: "Notifications",
                column: "ChapterId");

            migrationBuilder.AddForeignKey(
                name: "FK_Notifications_Chapters_ChapterId",
                table: "Notifications",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Notifications_Chapters_ChapterId",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_ChapterId",
                table: "Notifications");

            migrationBuilder.DropColumn(
                name: "ChapterId",
                table: "Notifications");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChapterEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chapter_Volumes_VolumeId",
                table: "Chapter");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterVersion_Chapter_ChapterId",
                table: "ChapterVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Chapter_ChapterId",
                table: "Comment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chapter",
                table: "Chapter");

            migrationBuilder.RenameTable(
                name: "Chapter",
                newName: "Chapters");

            migrationBuilder.RenameIndex(
                name: "IX_Chapter_VolumeId",
                table: "Chapters",
                newName: "IX_Chapters_VolumeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chapters",
                table: "Chapters",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chapters_Volumes_VolumeId",
                table: "Chapters",
                column: "VolumeId",
                principalTable: "Volumes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterVersion_Chapters_ChapterId",
                table: "ChapterVersion",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Chapters_ChapterId",
                table: "Comment",
                column: "ChapterId",
                principalTable: "Chapters",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chapters_Volumes_VolumeId",
                table: "Chapters");

            migrationBuilder.DropForeignKey(
                name: "FK_ChapterVersion_Chapters_ChapterId",
                table: "ChapterVersion");

            migrationBuilder.DropForeignKey(
                name: "FK_Comment_Chapters_ChapterId",
                table: "Comment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Chapters",
                table: "Chapters");

            migrationBuilder.RenameTable(
                name: "Chapters",
                newName: "Chapter");

            migrationBuilder.RenameIndex(
                name: "IX_Chapters_VolumeId",
                table: "Chapter",
                newName: "IX_Chapter_VolumeId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Chapter",
                table: "Chapter",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Chapter_Volumes_VolumeId",
                table: "Chapter",
                column: "VolumeId",
                principalTable: "Volumes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ChapterVersion_Chapter_ChapterId",
                table: "ChapterVersion",
                column: "ChapterId",
                principalTable: "Chapter",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Comment_Chapter_ChapterId",
                table: "Comment",
                column: "ChapterId",
                principalTable: "Chapter",
                principalColumn: "Id");
        }
    }
}

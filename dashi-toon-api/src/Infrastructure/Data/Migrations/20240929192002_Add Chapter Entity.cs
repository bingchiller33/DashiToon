using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddChapterEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Order",
                table: "Chapter",
                newName: "ChapterNumber");

            migrationBuilder.AddColumn<int>(
                name: "ChapterCount",
                table: "Volumes",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "Created",
                table: "Chapter",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "Chapter",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LastModified",
                table: "Chapter",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "Chapter",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "PublishedDate",
                table: "Chapter",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Thumbnail",
                table: "Chapter",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "VersionId",
                table: "Chapter",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateTable(
                name: "ChapterVersion",
                columns: table => new
                {
                    VersionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Thumbnail = table.Column<string>(type: "text", nullable: true),
                    Content = table.Column<string>(type: "text", nullable: false),
                    TimeStamp = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    ChapterId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChapterVersion", x => x.VersionId);
                    table.ForeignKey(
                        name: "FK_ChapterVersion_Chapter_ChapterId",
                        column: x => x.ChapterId,
                        principalTable: "Chapter",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChapterVersion_ChapterId",
                table: "ChapterVersion",
                column: "ChapterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ChapterVersion");

            migrationBuilder.DropColumn(
                name: "ChapterCount",
                table: "Volumes");

            migrationBuilder.DropColumn(
                name: "Created",
                table: "Chapter");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "Chapter");

            migrationBuilder.DropColumn(
                name: "LastModified",
                table: "Chapter");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "Chapter");

            migrationBuilder.DropColumn(
                name: "PublishedDate",
                table: "Chapter");

            migrationBuilder.DropColumn(
                name: "Thumbnail",
                table: "Chapter");

            migrationBuilder.DropColumn(
                name: "VersionId",
                table: "Chapter");

            migrationBuilder.RenameColumn(
                name: "ChapterNumber",
                table: "Chapter",
                newName: "Order");
        }
    }
}

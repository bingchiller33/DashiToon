using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChapterVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterVersion",
                table: "ChapterVersion");

            migrationBuilder.DropColumn(
                name: "VersionId",
                table: "ChapterVersion");

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterVersion",
                table: "ChapterVersion",
                column: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_ChapterVersion",
                table: "ChapterVersion");

            migrationBuilder.AddColumn<Guid>(
                name: "VersionId",
                table: "ChapterVersion",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddPrimaryKey(
                name: "PK_ChapterVersion",
                table: "ChapterVersion",
                column: "VersionId");
        }
    }
}

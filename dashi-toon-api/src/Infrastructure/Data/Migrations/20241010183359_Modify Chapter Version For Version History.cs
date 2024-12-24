using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifyChapterVersionForVersionHistory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Content",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "Thumbnail",
                table: "Chapters");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "Chapters");

            migrationBuilder.RenameColumn(
                name: "VersionId",
                table: "Chapters",
                newName: "CurrentVersionId");

            migrationBuilder.AddColumn<bool>(
                name: "IsAutoSave",
                table: "ChapterVersion",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "ChapterVersion",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "VersionName",
                table: "ChapterVersion",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "PublishedVersionId",
                table: "Chapters",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAutoSave",
                table: "ChapterVersion");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "ChapterVersion");

            migrationBuilder.DropColumn(
                name: "VersionName",
                table: "ChapterVersion");

            migrationBuilder.DropColumn(
                name: "PublishedVersionId",
                table: "Chapters");

            migrationBuilder.RenameColumn(
                name: "CurrentVersionId",
                table: "Chapters",
                newName: "VersionId");

            migrationBuilder.AddColumn<string>(
                name: "Content",
                table: "Chapters",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Chapters",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Chapters",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Thumbnail",
                table: "Chapters",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "Chapters",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");
        }
    }
}

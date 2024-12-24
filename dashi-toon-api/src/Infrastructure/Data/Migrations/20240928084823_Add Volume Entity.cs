using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddVolumeEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Order",
                table: "Volume",
                newName: "VolumeNumber");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "Volume",
                newName: "Introduction");

            migrationBuilder.AddColumn<int>(
                name: "VolumeCount",
                table: "Series",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VolumeCount",
                table: "Series");

            migrationBuilder.RenameColumn(
                name: "VolumeNumber",
                table: "Volume",
                newName: "Order");

            migrationBuilder.RenameColumn(
                name: "Introduction",
                table: "Volume",
                newName: "Description");
        }
    }
}

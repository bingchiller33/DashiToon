using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateRevenueTransaction1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropPrimaryKey(
                name: "PK_RevenueTransaction",
                table: "RevenueTransaction");

            migrationBuilder.AddColumn<int>(
                name: "SeriesId",
                table: "RevenueTransaction",
                type: "integer",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_RevenueTransaction",
                table: "RevenueTransaction",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueTransaction_AuthorId",
                table: "RevenueTransaction",
                column: "AuthorId");

            migrationBuilder.CreateIndex(
                name: "IX_RevenueTransaction_SeriesId",
                table: "RevenueTransaction",
                column: "SeriesId");

            migrationBuilder.AddForeignKey(
                name: "FK_RevenueTransaction_Series_SeriesId",
                table: "RevenueTransaction",
                column: "SeriesId",
                principalTable: "Series",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RevenueTransaction_Series_SeriesId",
                table: "RevenueTransaction");

            migrationBuilder.DropPrimaryKey(
                name: "PK_RevenueTransaction",
                table: "RevenueTransaction");

            migrationBuilder.DropIndex(
                name: "IX_RevenueTransaction_AuthorId",
                table: "RevenueTransaction");

            migrationBuilder.DropIndex(
                name: "IX_RevenueTransaction_SeriesId",
                table: "RevenueTransaction");

            migrationBuilder.DropColumn(
                name: "SeriesId",
                table: "RevenueTransaction");

            migrationBuilder.AddPrimaryKey(
                name: "PK_RevenueTransaction",
                table: "RevenueTransaction",
                columns: new[] { "AuthorId", "Id" });
        }
    }
}

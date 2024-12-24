using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddPurchaseOrdertoUserEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "PaymentStatus",
                table: "BillingDetail",
                newName: "BillingStatus");

            migrationBuilder.CreateTable(
                name: "PurchaseOrders",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    PackId = table.Column<Guid>(type: "uuid", nullable: false),
                    OrderInfo = table.Column<string>(type: "text", nullable: false),
                    OrderDetail = table.Column<string>(type: "text", nullable: true),
                    OrderPrice_Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    OrderPrice_Currency = table.Column<string>(type: "text", nullable: false),
                    Price_Amount = table.Column<decimal>(type: "numeric", nullable: true),
                    Price_Currency = table.Column<string>(type: "text", nullable: true),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    CompletedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PurchaseOrders", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_KanaGoldPacks_PackId",
                        column: x => x.PackId,
                        principalTable: "KanaGoldPacks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PurchaseOrders_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_PackId",
                table: "PurchaseOrders",
                column: "PackId");

            migrationBuilder.CreateIndex(
                name: "IX_PurchaseOrders_UserId",
                table: "PurchaseOrders",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PurchaseOrders");

            migrationBuilder.RenameColumn(
                name: "BillingStatus",
                table: "BillingDetail",
                newName: "PaymentStatus");
        }
    }
}

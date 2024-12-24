using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdatePurchaseOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OrderDetail",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OrderInfo",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OrderPrice_Amount",
                table: "PurchaseOrders");

            migrationBuilder.DropColumn(
                name: "OrderPrice_Currency",
                table: "PurchaseOrders");

            migrationBuilder.AlterColumn<string>(
                name: "Price_Currency",
                table: "PurchaseOrders",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price_Amount",
                table: "PurchaseOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Id",
                table: "PurchaseOrders",
                type: "text",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Price_Currency",
                table: "PurchaseOrders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price_Amount",
                table: "PurchaseOrders",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "PurchaseOrders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "OrderDetail",
                table: "PurchaseOrders",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "OrderInfo",
                table: "PurchaseOrders",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "OrderPrice_Amount",
                table: "PurchaseOrders",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "OrderPrice_Currency",
                table: "PurchaseOrders",
                type: "text",
                nullable: false,
                defaultValue: "");
        }
    }
}

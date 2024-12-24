using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSubscriptionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Subscription");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Subscription",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "BillingCycle_Interval",
                table: "DashiFans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "BillingCycle_IntervalCount",
                table: "DashiFans",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "BillingDetail",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Price_Amount = table.Column<decimal>(type: "numeric", nullable: false),
                    Price_Currency = table.Column<string>(type: "text", nullable: false),
                    BillingDate = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    IsPaid = table.Column<bool>(type: "boolean", nullable: false),
                    PaymentStatus = table.Column<int>(type: "integer", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BillingDetail", x => new { x.SubscriptionId, x.Id });
                    table.ForeignKey(
                        name: "FK_BillingDetail_Subscription_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscription",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BillingDetail");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Subscription");

            migrationBuilder.DropColumn(
                name: "BillingCycle_Interval",
                table: "DashiFans");

            migrationBuilder.DropColumn(
                name: "BillingCycle_IntervalCount",
                table: "DashiFans");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Subscription",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class ModifySubscriptionEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingDetail_Subscription_SubscriptionId",
                table: "BillingDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_DashiFans_DashiFanId",
                table: "Subscription");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscription_Users_UserId",
                table: "Subscription");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscription",
                table: "Subscription");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "BillingDetail");

            migrationBuilder.RenameTable(
                name: "Subscription",
                newName: "Subscriptions");

            migrationBuilder.RenameIndex(
                name: "IX_Subscription_UserId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscription_DashiFanId",
                table: "Subscriptions",
                newName: "IX_Subscriptions_DashiFanId");

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "NextBillingDate",
                table: "Subscriptions",
                type: "timestamp with time zone",
                nullable: true,
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingDetail_Subscriptions_SubscriptionId",
                table: "BillingDetail",
                column: "SubscriptionId",
                principalTable: "Subscriptions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_DashiFans_DashiFanId",
                table: "Subscriptions",
                column: "DashiFanId",
                principalTable: "DashiFans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                table: "Subscriptions",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BillingDetail_Subscriptions_SubscriptionId",
                table: "BillingDetail");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_DashiFans_DashiFanId",
                table: "Subscriptions");

            migrationBuilder.DropForeignKey(
                name: "FK_Subscriptions_Users_UserId",
                table: "Subscriptions");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Subscriptions",
                table: "Subscriptions");

            migrationBuilder.RenameTable(
                name: "Subscriptions",
                newName: "Subscription");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_UserId",
                table: "Subscription",
                newName: "IX_Subscription_UserId");

            migrationBuilder.RenameIndex(
                name: "IX_Subscriptions_DashiFanId",
                table: "Subscription",
                newName: "IX_Subscription_DashiFanId");

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "BillingDetail",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<DateTimeOffset>(
                name: "NextBillingDate",
                table: "Subscription",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)),
                oldClrType: typeof(DateTimeOffset),
                oldType: "timestamp with time zone",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Subscription",
                table: "Subscription",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_BillingDetail_Subscription_SubscriptionId",
                table: "BillingDetail",
                column: "SubscriptionId",
                principalTable: "Subscription",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_DashiFans_DashiFanId",
                table: "Subscription",
                column: "DashiFanId",
                principalTable: "DashiFans",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Subscription_Users_UserId",
                table: "Subscription",
                column: "UserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

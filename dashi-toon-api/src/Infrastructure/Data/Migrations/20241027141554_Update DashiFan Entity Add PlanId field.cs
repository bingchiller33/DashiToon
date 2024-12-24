using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DashiToon.Api.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDashiFanEntityAddPlanIdfield : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PlanId",
                table: "DashiFans",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PlanId",
                table: "DashiFans");
        }
    }
}

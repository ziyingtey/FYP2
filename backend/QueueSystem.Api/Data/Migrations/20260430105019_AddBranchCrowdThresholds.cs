using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueueSystem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddBranchCrowdThresholds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CrowdHighStartsAtPercent",
                table: "branch",
                type: "INTEGER",
                nullable: false,
                defaultValue: 70);

            migrationBuilder.AddColumn<int>(
                name: "CrowdMediumStartsAtPercent",
                table: "branch",
                type: "INTEGER",
                nullable: false,
                defaultValue: 40);

            migrationBuilder.AddColumn<int>(
                name: "OvercrowdStartsAtPercent",
                table: "branch",
                type: "INTEGER",
                nullable: false,
                defaultValue: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CrowdHighStartsAtPercent",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "CrowdMediumStartsAtPercent",
                table: "branch");

            migrationBuilder.DropColumn(
                name: "OvercrowdStartsAtPercent",
                table: "branch");
        }
    }
}

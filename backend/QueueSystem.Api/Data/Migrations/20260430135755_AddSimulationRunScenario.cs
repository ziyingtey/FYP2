using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueueSystem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSimulationRunScenario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Scenario",
                table: "simulation_run",
                type: "TEXT",
                maxLength: 64,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scenario",
                table: "simulation_run");
        }
    }
}

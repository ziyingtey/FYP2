using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QueueSystem.Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class StaffPortalAuth : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "LoginEmail",
                table: "staff",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "staff",
                type: "TEXT",
                maxLength: 512,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_staff_LoginEmail",
                table: "staff",
                column: "LoginEmail",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_staff_LoginEmail",
                table: "staff");

            migrationBuilder.DropColumn(
                name: "LoginEmail",
                table: "staff");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "staff");
        }
    }
}

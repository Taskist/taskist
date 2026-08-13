#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Taskist.Data.Migrations
{
    /// <inheritdoc />
    public partial class SecurityHardening : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "HashFormat",
                table: "UserPassword",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndDate",
                table: "User",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "HashFormat",
                table: "UserPassword");

            migrationBuilder.DropColumn(
                name: "LockoutEndDate",
                table: "User");
        }
    }
}

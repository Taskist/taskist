#nullable disable

using Microsoft.EntityFrameworkCore.Migrations;

namespace Taskist.Data.Migrations
{
    /// <inheritdoc />
    public partial class AvatarVersion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AvatarVersion",
                table: "User",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarVersion",
                table: "User");
        }
    }
}

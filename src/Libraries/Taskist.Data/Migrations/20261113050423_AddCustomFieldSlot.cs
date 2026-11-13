using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskist.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomFieldSlot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DisplayOrder",
                table: "CustomField",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Slot",
                table: "CustomField",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DisplayOrder",
                table: "CustomField");

            migrationBuilder.DropColumn(
                name: "Slot",
                table: "CustomField");
        }
    }
}

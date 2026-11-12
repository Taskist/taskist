using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskist.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTwoFactor : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserRecoveryCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    CodeHash = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UsedOn = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserRecoveryCode", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserRecoveryCode_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "UserTwoFactor",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<int>(type: "int", nullable: false),
                    Secret = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EnabledOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsedTimeStep = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTwoFactor", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTwoFactor_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserRecoveryCode_UserId",
                table: "UserRecoveryCode",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTwoFactor_UserId",
                table: "UserTwoFactor",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserRecoveryCode");

            migrationBuilder.DropTable(
                name: "UserTwoFactor");
        }
    }
}

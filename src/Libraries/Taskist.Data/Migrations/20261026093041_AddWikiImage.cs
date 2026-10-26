using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Taskist.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWikiImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WikiImage",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DocumentId = table.Column<int>(type: "int", nullable: false),
                    ProjectId = table.Column<int>(type: "int", nullable: false),
                    WikiDocumentId = table.Column<int>(type: "int", nullable: false),
                    UploadedById = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WikiImage", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WikiImage_Document_DocumentId",
                        column: x => x.DocumentId,
                        principalTable: "Document",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WikiImage_Project_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Project",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WikiImage_User_UploadedById",
                        column: x => x.UploadedById,
                        principalTable: "User",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_WikiImage_WikiDocument_WikiDocumentId",
                        column: x => x.WikiDocumentId,
                        principalTable: "WikiDocument",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_WikiImage_DocumentId",
                table: "WikiImage",
                column: "DocumentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WikiImage_ProjectId",
                table: "WikiImage",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_WikiImage_UploadedById",
                table: "WikiImage",
                column: "UploadedById");

            migrationBuilder.CreateIndex(
                name: "IX_WikiImage_WikiDocumentId",
                table: "WikiImage",
                column: "WikiDocumentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WikiImage");
        }
    }
}

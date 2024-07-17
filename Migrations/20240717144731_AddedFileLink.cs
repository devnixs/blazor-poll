using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Poll.Migrations
{
    /// <inheritdoc />
    public partial class AddedFileLink : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GameTemplateId",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateIndex(
                name: "IX_Files_GameTemplateId",
                table: "Files",
                column: "GameTemplateId");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_GameTemplates_GameTemplateId",
                table: "Files",
                column: "GameTemplateId",
                principalTable: "GameTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_GameTemplates_GameTemplateId",
                table: "Files");

            migrationBuilder.DropIndex(
                name: "IX_Files_GameTemplateId",
                table: "Files");

            migrationBuilder.DropColumn(
                name: "GameTemplateId",
                table: "Files");
        }
    }
}

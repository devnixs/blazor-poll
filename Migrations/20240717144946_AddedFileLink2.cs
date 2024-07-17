using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Poll.Migrations
{
    /// <inheritdoc />
    public partial class AddedFileLink2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_GameTemplates_GameTemplateId",
                table: "Files");

            migrationBuilder.AlterColumn<int>(
                name: "GameTemplateId",
                table: "Files",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Files_GameTemplates_GameTemplateId",
                table: "Files",
                column: "GameTemplateId",
                principalTable: "GameTemplates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Files_GameTemplates_GameTemplateId",
                table: "Files");

            migrationBuilder.AlterColumn<int>(
                name: "GameTemplateId",
                table: "Files",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Files_GameTemplates_GameTemplateId",
                table: "Files",
                column: "GameTemplateId",
                principalTable: "GameTemplates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}

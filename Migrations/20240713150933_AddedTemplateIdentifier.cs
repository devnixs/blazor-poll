using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Poll.Migrations
{
    /// <inheritdoc />
    public partial class AddedTemplateIdentifier : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "GameTemplates",
                type: "character varying(256)",
                maxLength: 256,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AddColumn<string>(
                name: "Identifier",
                table: "GameTemplates",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_GameTemplates_Identifier",
                table: "GameTemplates",
                column: "Identifier");

            migrationBuilder.Sql("""UPDATE "GameTemplates" SET "Identifier" = "Id" """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_GameTemplates_Identifier",
                table: "GameTemplates");

            migrationBuilder.DropColumn(
                name: "Identifier",
                table: "GameTemplates");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "GameTemplates",
                type: "text",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(256)",
                oldMaxLength: 256,
                oldNullable: true);
        }
    }
}

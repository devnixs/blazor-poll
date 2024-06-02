using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Poll.Migrations
{
    /// <inheritdoc />
    public partial class AddedImages : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AskingQuestionImageUrl",
                table: "Questions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PresentingAnswerImageUrl",
                table: "Questions",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AskingQuestionImageUrl",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PresentingAnswerImageUrl",
                table: "Questions");
        }
    }
}

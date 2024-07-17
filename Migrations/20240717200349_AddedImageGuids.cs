using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Poll.Migrations
{
    /// <inheritdoc />
    public partial class AddedImageGuids : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "AskingQuestionImageId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PresentingAnswerImageId",
                table: "Questions",
                type: "uuid",
                nullable: true);

            migrationBuilder.Sql(""""
                                 update "Questions" set "AskingQuestionImageUrl" = null where "AskingQuestionImageUrl" like 'https://%';
                                 update "Questions"
                                 set "AskingQuestionImageId" = replace("AskingQuestionImageUrl", '/file/get/','')::uuid
                                 where "AskingQuestionImageUrl" like '/file/get/%';
                                 """");
            migrationBuilder.Sql(""""
                                 update "Questions" set "PresentingAnswerImageUrl" = null where "PresentingAnswerImageUrl" like 'https://%';
                                 update "Questions"
                                 set "PresentingAnswerImageId" = replace("PresentingAnswerImageUrl", '/file/get/','')::uuid
                                 where "PresentingAnswerImageUrl" like '/file/get/%';
                                 """");
            
            migrationBuilder.DropColumn(
                name: "AskingQuestionImageUrl",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PresentingAnswerImageUrl",
                table: "Questions");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AskingQuestionImageId",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "PresentingAnswerImageId",
                table: "Questions");

            migrationBuilder.AddColumn<string>(
                name: "AskingQuestionImageUrl",
                table: "Questions",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PresentingAnswerImageUrl",
                table: "Questions",
                type: "character varying(1024)",
                maxLength: 1024,
                nullable: true);
        }
    }
}

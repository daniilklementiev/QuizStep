using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace QuizStep.Migrations
{
    /// <inheritdoc />
    public partial class StudentAnswersFixes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsRight",
                table: "StudentAnswers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "TestId",
                table: "StudentAnswers",
                type: "char(36)",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                collation: "ascii_general_ci");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsRight",
                table: "StudentAnswers");

            migrationBuilder.DropColumn(
                name: "TestId",
                table: "StudentAnswers");
        }
    }
}

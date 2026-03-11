using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Virtual_Call_Project_Testing.Migrations
{
    /// <inheritdoc />
    public partial class add_table_attempt_questions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizAttempts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizMasterId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    AnsweredQuestions = table.Column<int>(type: "int", nullable: false),
                    Score = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizAttempts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_QuizMasters_QuizMasterId",
                        column: x => x.QuizMasterId,
                        principalTable: "QuizMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizAttempts_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_QuizMasterId",
                table: "QuizAttempts",
                column: "QuizMasterId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizAttempts_StudentId",
                table: "QuizAttempts",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizAttempts");
        }
    }
}

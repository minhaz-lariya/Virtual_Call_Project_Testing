using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Virtual_Call_Project_Testing.Migrations
{
    /// <inheritdoc />
    public partial class question_master_add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuestionMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    QuizMasterId = table.Column<int>(type: "int", nullable: false),
                    QuestionText = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Option1 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Option2 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Option3 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Option4 = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Answer = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuestionMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuestionMasters_QuizMasters_QuizMasterId",
                        column: x => x.QuizMasterId,
                        principalTable: "QuizMasters",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuestionMasters_QuizMasterId",
                table: "QuestionMasters",
                column: "QuizMasterId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuestionMasters");
        }
    }
}

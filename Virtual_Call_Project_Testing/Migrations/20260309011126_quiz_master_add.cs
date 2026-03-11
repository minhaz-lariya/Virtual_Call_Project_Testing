using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Virtual_Call_Project_Testing.Migrations
{
    /// <inheritdoc />
    public partial class quiz_master_add : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "QuizMasters",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SubjectId = table.Column<int>(type: "int", nullable: false),
                    FacultyId = table.Column<int>(type: "int", nullable: false),
                    QuizTitle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuizDescription = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    QuizStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    QuizEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TotalQuestions = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_QuizMasters", x => x.Id);
                    table.ForeignKey(
                        name: "FK_QuizMasters_Faculties_FacultyId",
                        column: x => x.FacultyId,
                        principalTable: "Faculties",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_QuizMasters_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_QuizMasters_FacultyId",
                table: "QuizMasters",
                column: "FacultyId");

            migrationBuilder.CreateIndex(
                name: "IX_QuizMasters_SubjectId",
                table: "QuizMasters",
                column: "SubjectId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "QuizMasters");
        }
    }
}

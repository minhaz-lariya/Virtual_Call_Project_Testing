using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Virtual_Call_Project_Testing.Migrations
{
    /// <inheritdoc />
    public partial class add_table_submit_assignments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AssignmentSubmits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AssignmentsId = table.Column<int>(type: "int", nullable: false),
                    StudentId = table.Column<int>(type: "int", nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Remarks = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MarksObtained = table.Column<int>(type: "int", nullable: true),
                    SubmittedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    uploadedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssignmentSubmits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmits_Assignments_AssignmentsId",
                        column: x => x.AssignmentsId,
                        principalTable: "Assignments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssignmentSubmits_Students_StudentId",
                        column: x => x.StudentId,
                        principalTable: "Students",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmits_AssignmentsId",
                table: "AssignmentSubmits",
                column: "AssignmentsId");

            migrationBuilder.CreateIndex(
                name: "IX_AssignmentSubmits_StudentId",
                table: "AssignmentSubmits",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssignmentSubmits");
        }
    }
}

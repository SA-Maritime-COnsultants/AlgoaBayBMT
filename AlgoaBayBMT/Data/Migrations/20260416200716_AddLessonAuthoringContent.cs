using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLessonAuthoringContent : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AuthoringLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthoringLessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LessonContentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FrontText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BackHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    QuestionHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScenarioHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionA = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OptionB = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OptionC = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OptionD = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Option1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Option2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Option3 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Option4 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CorrectOption = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CorrectBool = table.Column<bool>(type: "bit", nullable: true),
                    CorrectOptionIndex = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonContentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonContentItems_AuthoringLessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "AuthoringLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LessonContentItems_LessonId_ContentType",
                table: "LessonContentItems",
                columns: new[] { "LessonId", "ContentType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LessonContentItems");

            migrationBuilder.DropTable(
                name: "AuthoringLessons");
        }
    }
}

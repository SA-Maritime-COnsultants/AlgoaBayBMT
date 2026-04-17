using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTrainingPlatform : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "KnowledgeCheckPassed",
                table: "UserLessonProgress",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "LessonBlocks",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Subtitle",
                table: "LessonBlocks",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ThumbnailUrl",
                table: "LessonBlocks",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PassMarkPercent",
                table: "Courses",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: false,
                defaultValue: 80m);

            migrationBuilder.AddColumn<string>(
                name: "Summary",
                table: "Courses",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TrainingCourseAssessments",
                columns: table => new
                {
                    TrainingCourseAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PassMarkPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 80m),
                    RandomQuestionCount = table.Column<int>(type: "int", nullable: false, defaultValue: 25),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingCourseAssessments", x => x.TrainingCourseAssessmentId);
                    table.ForeignKey(
                        name: "FK_TrainingCourseAssessments_Courses_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "TrainingKnowledgeCheckQuestions",
                columns: table => new
                {
                    TrainingKnowledgeCheckQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingLessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 1m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingKnowledgeCheckQuestions", x => x.TrainingKnowledgeCheckQuestionId);
                    table.ForeignKey(
                        name: "FK_TrainingKnowledgeCheckQuestions_Lessons_TrainingLessonId",
                        column: x => x.TrainingLessonId,
                        principalTable: "Lessons",
                        principalColumn: "LessonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingQuestionBankQuestions",
                columns: table => new
                {
                    TrainingQuestionBankQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScenarioText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DifficultyLevel = table.Column<int>(type: "int", nullable: true),
                    Points = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 1m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingQuestionBankQuestions", x => x.TrainingQuestionBankQuestionId);
                    table.ForeignKey(
                        name: "FK_TrainingQuestionBankQuestions_Modules_TrainingModuleId",
                        column: x => x.TrainingModuleId,
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrainingQuestionBankQuestions_TrainingCourseAssessments_TrainingCourseAssessmentId",
                        column: x => x.TrainingCourseAssessmentId,
                        principalTable: "TrainingCourseAssessments",
                        principalColumn: "TrainingCourseAssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAssessmentAttempts",
                columns: table => new
                {
                    UserAssessmentAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    StartedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScorePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Passed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssessmentAttempts", x => x.UserAssessmentAttemptId);
                    table.ForeignKey(
                        name: "FK_UserAssessmentAttempts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAssessmentAttempts_TrainingCourseAssessments_TrainingCourseAssessmentId",
                        column: x => x.TrainingCourseAssessmentId,
                        principalTable: "TrainingCourseAssessments",
                        principalColumn: "TrainingCourseAssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingKnowledgeCheckOptions",
                columns: table => new
                {
                    TrainingKnowledgeCheckOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingKnowledgeCheckQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingKnowledgeCheckOptions", x => x.TrainingKnowledgeCheckOptionId);
                    table.ForeignKey(
                        name: "FK_TrainingKnowledgeCheckOptions_TrainingKnowledgeCheckQuestions_TrainingKnowledgeCheckQuestionId",
                        column: x => x.TrainingKnowledgeCheckQuestionId,
                        principalTable: "TrainingKnowledgeCheckQuestions",
                        principalColumn: "TrainingKnowledgeCheckQuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingQuestionBankOptions",
                columns: table => new
                {
                    TrainingQuestionBankOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingQuestionBankQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingQuestionBankOptions", x => x.TrainingQuestionBankOptionId);
                    table.ForeignKey(
                        name: "FK_TrainingQuestionBankOptions_TrainingQuestionBankQuestions_TrainingQuestionBankQuestionId",
                        column: x => x.TrainingQuestionBankQuestionId,
                        principalTable: "TrainingQuestionBankQuestions",
                        principalColumn: "TrainingQuestionBankQuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAssessmentResponses",
                columns: table => new
                {
                    UserAssessmentResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserAssessmentAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingQuestionBankQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FreeTextAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    AwardedPoints = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssessmentResponses", x => x.UserAssessmentResponseId);
                    table.ForeignKey(
                        name: "FK_UserAssessmentResponses_TrainingQuestionBankOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "TrainingQuestionBankOptions",
                        principalColumn: "TrainingQuestionBankOptionId");
                    table.ForeignKey(
                        name: "FK_UserAssessmentResponses_TrainingQuestionBankQuestions_TrainingQuestionBankQuestionId",
                        column: x => x.TrainingQuestionBankQuestionId,
                        principalTable: "TrainingQuestionBankQuestions",
                        principalColumn: "TrainingQuestionBankQuestionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAssessmentResponses_UserAssessmentAttempts_UserAssessmentAttemptId",
                        column: x => x.UserAssessmentAttemptId,
                        principalTable: "UserAssessmentAttempts",
                        principalColumn: "UserAssessmentAttemptId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourseAssessments_TrainingCourseId",
                table: "TrainingCourseAssessments",
                column: "TrainingCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingKnowledgeCheckOptions_TrainingKnowledgeCheckQuestionId_OrderIndex",
                table: "TrainingKnowledgeCheckOptions",
                columns: new[] { "TrainingKnowledgeCheckQuestionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingKnowledgeCheckQuestions_TrainingLessonId_OrderIndex",
                table: "TrainingKnowledgeCheckQuestions",
                columns: new[] { "TrainingLessonId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionBankOptions_TrainingQuestionBankQuestionId_OrderIndex",
                table: "TrainingQuestionBankOptions",
                columns: new[] { "TrainingQuestionBankQuestionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionBankQuestions_TrainingCourseAssessmentId",
                table: "TrainingQuestionBankQuestions",
                column: "TrainingCourseAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionBankQuestions_TrainingModuleId",
                table: "TrainingQuestionBankQuestions",
                column: "TrainingModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAttempts_TrainingCourseAssessmentId_UserId_AttemptNumber",
                table: "UserAssessmentAttempts",
                columns: new[] { "TrainingCourseAssessmentId", "UserId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAttempts_UserId",
                table: "UserAssessmentAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentResponses_SelectedOptionId",
                table: "UserAssessmentResponses",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentResponses_TrainingQuestionBankQuestionId",
                table: "UserAssessmentResponses",
                column: "TrainingQuestionBankQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentResponses_UserAssessmentAttemptId",
                table: "UserAssessmentResponses",
                column: "UserAssessmentAttemptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TrainingKnowledgeCheckOptions");

            migrationBuilder.DropTable(
                name: "UserAssessmentResponses");

            migrationBuilder.DropTable(
                name: "TrainingKnowledgeCheckQuestions");

            migrationBuilder.DropTable(
                name: "TrainingQuestionBankOptions");

            migrationBuilder.DropTable(
                name: "UserAssessmentAttempts");

            migrationBuilder.DropTable(
                name: "TrainingQuestionBankQuestions");

            migrationBuilder.DropTable(
                name: "TrainingCourseAssessments");

            migrationBuilder.DropColumn(
                name: "KnowledgeCheckPassed",
                table: "UserLessonProgress");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "LessonBlocks");

            migrationBuilder.DropColumn(
                name: "Subtitle",
                table: "LessonBlocks");

            migrationBuilder.DropColumn(
                name: "ThumbnailUrl",
                table: "LessonBlocks");

            migrationBuilder.DropColumn(
                name: "PassMarkPercent",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Summary",
                table: "Courses");
        }
    }
}

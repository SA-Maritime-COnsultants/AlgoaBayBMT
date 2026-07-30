using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class TrainingConsolidation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AssessmentResponses");

            migrationBuilder.DropTable(
                name: "TrainingKnowledgeCheckOptions");

            migrationBuilder.DropTable(
                name: "AssessmentAttempts");

            migrationBuilder.DropTable(
                name: "AssessmentOptions");

            migrationBuilder.DropTable(
                name: "TrainingKnowledgeCheckQuestions");

            migrationBuilder.DropTable(
                name: "AssessmentQuestions");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropIndex(
                name: "IX_Modules_CourseVersionId_OrderIndex",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_ModuleId_OrderIndex",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_LessonBlocks_LessonId_OrderIndex",
                table: "LessonBlocks");

            migrationBuilder.AddColumn<Guid>(
                name: "TrainingLessonId",
                table: "TrainingQuestionBankQuestions",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionBankQuestions_TrainingLessonId",
                table: "TrainingQuestionBankQuestions",
                column: "TrainingLessonId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_CourseVersionId_OrderIndex",
                table: "Modules",
                columns: new[] { "CourseVersionId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ModuleId_OrderIndex",
                table: "Lessons",
                columns: new[] { "ModuleId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_LessonBlocks_LessonId_OrderIndex",
                table: "LessonBlocks",
                columns: new[] { "LessonId", "OrderIndex" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_TrainingQuestionBankQuestions_TrainingLessonId",
                table: "TrainingQuestionBankQuestions");

            migrationBuilder.DropIndex(
                name: "IX_Modules_CourseVersionId_OrderIndex",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Lessons_ModuleId_OrderIndex",
                table: "Lessons");

            migrationBuilder.DropIndex(
                name: "IX_LessonBlocks_LessonId_OrderIndex",
                table: "LessonBlocks");

            migrationBuilder.DropColumn(
                name: "TrainingLessonId",
                table: "TrainingQuestionBankQuestions");

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    PassMarkPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 80m),
                    RandomizeQuestions = table.Column<bool>(type: "bit", nullable: false),
                    ShowFeedbackAfterSubmit = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: true),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.AssessmentId);
                    table.ForeignKey(
                        name: "FK_Assessments_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "LessonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingKnowledgeCheckQuestions",
                columns: table => new
                {
                    TrainingKnowledgeCheckQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingLessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 1m),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false)
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
                name: "AssessmentAttempts",
                columns: table => new
                {
                    AssessmentAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    Passed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ScorePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    StartedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAttempts", x => x.AssessmentAttemptId);
                    table.ForeignKey(
                        name: "FK_AssessmentAttempts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentAttempts_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestions",
                columns: table => new
                {
                    AssessmentQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ExplanationMarkdown = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 1m),
                    PromptMarkdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestions", x => x.AssessmentQuestionId);
                    table.ForeignKey(
                        name: "FK_AssessmentQuestions_Assessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "Assessments",
                        principalColumn: "AssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingKnowledgeCheckOptions",
                columns: table => new
                {
                    TrainingKnowledgeCheckOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingKnowledgeCheckQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OptionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
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
                name: "AssessmentOptions",
                columns: table => new
                {
                    AssessmentOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OptionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentOptions", x => x.AssessmentOptionId);
                    table.ForeignKey(
                        name: "FK_AssessmentOptions_AssessmentQuestions_AssessmentQuestionId",
                        column: x => x.AssessmentQuestionId,
                        principalTable: "AssessmentQuestions",
                        principalColumn: "AssessmentQuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentResponses",
                columns: table => new
                {
                    AssessmentResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AwardedPoints = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true),
                    FreeTextAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentResponses", x => x.AssessmentResponseId);
                    table.ForeignKey(
                        name: "FK_AssessmentResponses_AssessmentAttempts_AssessmentAttemptId",
                        column: x => x.AssessmentAttemptId,
                        principalTable: "AssessmentAttempts",
                        principalColumn: "AssessmentAttemptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentResponses_AssessmentOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "AssessmentOptions",
                        principalColumn: "AssessmentOptionId");
                    table.ForeignKey(
                        name: "FK_AssessmentResponses_AssessmentQuestions_AssessmentQuestionId",
                        column: x => x.AssessmentQuestionId,
                        principalTable: "AssessmentQuestions",
                        principalColumn: "AssessmentQuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Modules_CourseVersionId_OrderIndex",
                table: "Modules",
                columns: new[] { "CourseVersionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ModuleId_OrderIndex",
                table: "Lessons",
                columns: new[] { "ModuleId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonBlocks_LessonId_OrderIndex",
                table: "LessonBlocks",
                columns: new[] { "LessonId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempts_AssessmentId_UserId_AttemptNumber",
                table: "AssessmentAttempts",
                columns: new[] { "AssessmentId", "UserId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempts_UserId",
                table: "AssessmentAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentOptions_AssessmentQuestionId_OrderIndex",
                table: "AssessmentOptions",
                columns: new[] { "AssessmentQuestionId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestions_AssessmentId_OrderIndex",
                table: "AssessmentQuestions",
                columns: new[] { "AssessmentId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResponses_AssessmentAttemptId",
                table: "AssessmentResponses",
                column: "AssessmentAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResponses_AssessmentQuestionId",
                table: "AssessmentResponses",
                column: "AssessmentQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResponses_SelectedOptionId",
                table: "AssessmentResponses",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_LessonId",
                table: "Assessments",
                column: "LessonId",
                unique: true);

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
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTRainingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletionRule",
                table: "Lessons");

            migrationBuilder.DropColumn(
                name: "LessonType",
                table: "Lessons");

            migrationBuilder.AddColumn<Guid>(
                name: "AssessmentId",
                table: "Modules",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AssessmentMaxAttempts",
                table: "Modules",
                type: "int",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<decimal>(
                name: "AssessmentPassMarkPercent",
                table: "Modules",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasModuleAssessment",
                table: "Modules",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "Lessons",
                type: "bit",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "bit",
                oldDefaultValue: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_AssessmentId",
                table: "Modules",
                column: "AssessmentId");

            migrationBuilder.AddForeignKey(
                name: "FK_Modules_TrainingCourseAssessments_AssessmentId",
                table: "Modules",
                column: "AssessmentId",
                principalTable: "TrainingCourseAssessments",
                principalColumn: "TrainingCourseAssessmentId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Modules_TrainingCourseAssessments_AssessmentId",
                table: "Modules");

            migrationBuilder.DropIndex(
                name: "IX_Modules_AssessmentId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "AssessmentId",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "AssessmentMaxAttempts",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "AssessmentPassMarkPercent",
                table: "Modules");

            migrationBuilder.DropColumn(
                name: "HasModuleAssessment",
                table: "Modules");

            migrationBuilder.AlterColumn<bool>(
                name: "IsRequired",
                table: "Lessons",
                type: "bit",
                nullable: false,
                defaultValue: true,
                oldClrType: typeof(bool),
                oldType: "bit");

            migrationBuilder.AddColumn<int>(
                name: "CompletionRule",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "LessonType",
                table: "Lessons",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }
    }
}

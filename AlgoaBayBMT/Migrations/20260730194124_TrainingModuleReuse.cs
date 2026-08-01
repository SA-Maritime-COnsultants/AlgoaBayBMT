using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class TrainingModuleReuse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ---------------------------------------------------------------------------------
            // STEP 1 - Purge existing TRAINING CONTENT ONLY.
            //
            // Authorised explicitly: the training catalogue is being rebuilt from scratch under
            // the shared-module model, and the handful of existing rows are throwaway test data.
            //
            // The ASP.NET Identity tables - AspNetUsers, AspNetRoles, AspNetUserRoles and the rest
            // of the AspNet* family - are DELIBERATELY NOT TOUCHED, so existing logins and role
            // assignments survive this migration. Neither are CrewMembers, Vessels or Invoices;
            // only the training rows that reference them are removed.
            //
            // Order follows the foreign keys, children first. Cross-module references into
            // training are nulled before their targets disappear.
            // ---------------------------------------------------------------------------------
            migrationBuilder.Sql(@"
                -- Release cross-module references into training before deleting the targets.
                UPDATE [InvoiceLineItems]      SET [RelatedCourseId] = NULL         WHERE [RelatedCourseId] IS NOT NULL;
                UPDATE [ComplianceRules]       SET [RequiredCourseId] = NULL        WHERE [RequiredCourseId] IS NOT NULL;
                UPDATE [CertificateExpiryEvents] SET [TrainingCertificateId] = NULL WHERE [TrainingCertificateId] IS NOT NULL;

                -- Break the Course -> CurrentVersion cycle so CourseVersions can be deleted.
                UPDATE [Courses] SET [CurrentVersionId] = NULL WHERE [CurrentVersionId] IS NOT NULL;

                -- Learner evidence and attempts.
                DELETE FROM [UserAssessmentResponses];
                DELETE FROM [UserAssessmentAttempts];
                DELETE FROM [TrainingCertificates];
                DELETE FROM [CourseCompletionRecords];
                DELETE FROM [UserLessonProgress];
                DELETE FROM [UserCourseProgress];
                DELETE FROM [UserTrainingAssignments];

                -- Question bank.
                DELETE FROM [TrainingQuestionBankOptions];
                DELETE FROM [TrainingQuestionBankQuestions];
                DELETE FROM [TrainingCourseAssessments];

                -- Authored content and catalogue.
                DELETE FROM [LessonBlocks];
                DELETE FROM [Lessons];
                DELETE FROM [Modules];
                DELETE FROM [CourseAudienceRules];
                DELETE FROM [CourseVersions];
                DELETE FROM [Courses];

                -- Training audit history refers to identifiers that no longer exist.
                DELETE FROM [TrainingAuditLogs];
            ");

            // ---------------------------------------------------------------------------------
            // STEP 2 - Restructure.
            //
            // The old [Modules] table held authored CONTENT owned by exactly one course version.
            // Under the new model that content lives in [ModuleVersions] (created further down)
            // and [TrainingModules] is a NEW table holding stable, course-independent identity.
            // They are not the same thing, so [Modules] is dropped rather than renamed - the
            // scaffolder's rename would have silently turned content rows into identity rows.
            // Both tables are empty after Step 1, so nothing is lost.
            // ---------------------------------------------------------------------------------
            migrationBuilder.DropForeignKey(
                name: "FK_Lessons_Modules_ModuleId",
                table: "Lessons");

            migrationBuilder.DropForeignKey(
                name: "FK_TrainingQuestionBankQuestions_Modules_TrainingModuleId",
                table: "TrainingQuestionBankQuestions");

            migrationBuilder.DropTable(name: "Modules");

            migrationBuilder.RenameColumn(
                name: "TrainingModuleId",
                table: "TrainingQuestionBankQuestions",
                newName: "TrainingModuleVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_TrainingQuestionBankQuestions_TrainingModuleId",
                table: "TrainingQuestionBankQuestions",
                newName: "IX_TrainingQuestionBankQuestions_TrainingModuleVersionId");

            migrationBuilder.RenameColumn(
                name: "ModuleId",
                table: "Lessons",
                newName: "ModuleVersionId");

            migrationBuilder.RenameIndex(
                name: "IX_Lessons_ModuleId_OrderIndex",
                table: "Lessons",
                newName: "IX_Lessons_ModuleVersionId_OrderIndex");

            // Stable module identity. Created fresh - it has no predecessor in the old schema.
            migrationBuilder.CreateTable(
                name: "TrainingModules",
                columns: table => new
                {
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Category = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CurrentVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    ArchivedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ArchivedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingModules", x => x.ModuleId);
                });

            migrationBuilder.AddColumn<Guid>(
                name: "ResolvedCourseVersionId",
                table: "UserTrainingAssignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ResolvedOnUtc",
                table: "UserTrainingAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ResolvedRankProfileId",
                table: "UserTrainingAssignments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CourseVersionId",
                table: "UserCourseProgress",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RankProfileId",
                table: "UserCourseProgress",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleVersionId",
                table: "TrainingCourseAssessments",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourseCodeSnapshot",
                table: "TrainingCertificates",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourseTitleSnapshot",
                table: "TrainingCertificates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LearnerFullNameSnapshot",
                table: "TrainingCertificates",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RankProfileNameSnapshot",
                table: "TrainingCertificates",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VersionLabelSnapshot",
                table: "TrainingCertificates",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "CourseVersions",
                type: "rowversion",
                rowVersion: true,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RankProfileId",
                table: "CourseCompletionRecords",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RankProfileName",
                table: "CourseCompletionRecords",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ModuleVersions",
                columns: table => new
                {
                    ModuleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    VersionLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HasModuleAssessment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssessmentPassMarkPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AssessmentMaxAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    ChangeSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PublishedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PublishedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ModuleVersions", x => x.ModuleVersionId);
                    table.ForeignKey(
                        name: "FK_ModuleVersions_TrainingCourseAssessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "TrainingCourseAssessments",
                        principalColumn: "TrainingCourseAssessmentId");
                    table.ForeignKey(
                        name: "FK_ModuleVersions_TrainingModules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "TrainingModules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RankProfiles",
                columns: table => new
                {
                    RankProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankProfiles", x => x.RankProfileId);
                });

            migrationBuilder.CreateTable(
                name: "CourseModules",
                columns: table => new
                {
                    CourseModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseModules", x => x.CourseModuleId);
                    table.ForeignKey(
                        name: "FK_CourseModules_CourseVersions_CourseVersionId",
                        column: x => x.CourseVersionId,
                        principalTable: "CourseVersions",
                        principalColumn: "CourseVersionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseModules_ModuleVersions_ModuleVersionId",
                        column: x => x.ModuleVersionId,
                        principalTable: "ModuleVersions",
                        principalColumn: "ModuleVersionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CourseRankProfiles",
                columns: table => new
                {
                    CourseRankProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RankProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    TotalDurationMinutes = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRankProfiles", x => x.CourseRankProfileId);
                    table.ForeignKey(
                        name: "FK_CourseRankProfiles_CourseVersions_CourseVersionId",
                        column: x => x.CourseVersionId,
                        principalTable: "CourseVersions",
                        principalColumn: "CourseVersionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseRankProfiles_RankProfiles_RankProfileId",
                        column: x => x.RankProfileId,
                        principalTable: "RankProfiles",
                        principalColumn: "RankProfileId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "RankProfileRanks",
                columns: table => new
                {
                    RankProfileRankId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RankProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CrewRank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RankProfileRanks", x => x.RankProfileRankId);
                    table.ForeignKey(
                        name: "FK_RankProfileRanks_RankProfiles_RankProfileId",
                        column: x => x.RankProfileId,
                        principalTable: "RankProfiles",
                        principalColumn: "RankProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseRankModules",
                columns: table => new
                {
                    CourseRankModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseRankProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IsIncluded = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    RowVersion = table.Column<byte[]>(type: "rowversion", rowVersion: true, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseRankModules", x => x.CourseRankModuleId);
                    table.ForeignKey(
                        name: "FK_CourseRankModules_CourseModules_CourseModuleId",
                        column: x => x.CourseModuleId,
                        principalTable: "CourseModules",
                        principalColumn: "CourseModuleId");
                    table.ForeignKey(
                        name: "FK_CourseRankModules_CourseRankProfiles_CourseRankProfileId",
                        column: x => x.CourseRankProfileId,
                        principalTable: "CourseRankProfiles",
                        principalColumn: "CourseRankProfileId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_ResolvedCourseVersionId",
                table: "UserTrainingAssignments",
                column: "ResolvedCourseVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_ResolvedRankProfileId",
                table: "UserTrainingAssignments",
                column: "ResolvedRankProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCourseProgress_CourseVersionId",
                table: "UserCourseProgress",
                column: "CourseVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCourseProgress_RankProfileId",
                table: "UserCourseProgress",
                column: "RankProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourseAssessments_ModuleVersionId",
                table: "TrainingCourseAssessments",
                column: "ModuleVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseCompletionRecords_RankProfileId",
                table: "CourseCompletionRecords",
                column: "RankProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingModules_Category",
                table: "TrainingModules",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingModules_Code",
                table: "TrainingModules",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingModules_IsArchived",
                table: "TrainingModules",
                column: "IsArchived");

            migrationBuilder.CreateIndex(
                name: "IX_CourseModules_CourseVersionId_ModuleVersionId",
                table: "CourseModules",
                columns: new[] { "CourseVersionId", "ModuleVersionId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseModules_CourseVersionId_OrderIndex",
                table: "CourseModules",
                columns: new[] { "CourseVersionId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseModules_ModuleVersionId",
                table: "CourseModules",
                column: "ModuleVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRankModules_CourseModuleId",
                table: "CourseRankModules",
                column: "CourseModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseRankModules_CourseRankProfileId_CourseModuleId",
                table: "CourseRankModules",
                columns: new[] { "CourseRankProfileId", "CourseModuleId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseRankProfiles_CourseVersionId_OrderIndex",
                table: "CourseRankProfiles",
                columns: new[] { "CourseVersionId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseRankProfiles_CourseVersionId_RankProfileId",
                table: "CourseRankProfiles",
                columns: new[] { "CourseVersionId", "RankProfileId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseRankProfiles_RankProfileId",
                table: "CourseRankProfiles",
                column: "RankProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleVersions_AssessmentId",
                table: "ModuleVersions",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleVersions_ModuleId_VersionNumber",
                table: "ModuleVersions",
                columns: new[] { "ModuleId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ModuleVersions_Status",
                table: "ModuleVersions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_RankProfileRanks_CrewRank",
                table: "RankProfileRanks",
                column: "CrewRank");

            migrationBuilder.CreateIndex(
                name: "IX_RankProfileRanks_RankProfileId_CrewRank",
                table: "RankProfileRanks",
                columns: new[] { "RankProfileId", "CrewRank" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankProfiles_Code",
                table: "RankProfiles",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RankProfiles_OrderIndex",
                table: "RankProfiles",
                column: "OrderIndex");

            migrationBuilder.AddForeignKey(
                name: "FK_CourseCompletionRecords_RankProfiles_RankProfileId",
                table: "CourseCompletionRecords",
                column: "RankProfileId",
                principalTable: "RankProfiles",
                principalColumn: "RankProfileId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Lessons_ModuleVersions_ModuleVersionId",
                table: "Lessons",
                column: "ModuleVersionId",
                principalTable: "ModuleVersions",
                principalColumn: "ModuleVersionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingCourseAssessments_ModuleVersions_ModuleVersionId",
                table: "TrainingCourseAssessments",
                column: "ModuleVersionId",
                principalTable: "ModuleVersions",
                principalColumn: "ModuleVersionId");

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingModules_ModuleVersions_CurrentVersionId",
                table: "TrainingModules",
                column: "CurrentVersionId",
                principalTable: "ModuleVersions",
                principalColumn: "ModuleVersionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_TrainingQuestionBankQuestions_ModuleVersions_TrainingModuleVersionId",
                table: "TrainingQuestionBankQuestions",
                column: "TrainingModuleVersionId",
                principalTable: "ModuleVersions",
                principalColumn: "ModuleVersionId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCourseProgress_CourseVersions_CourseVersionId",
                table: "UserCourseProgress",
                column: "CourseVersionId",
                principalTable: "CourseVersions",
                principalColumn: "CourseVersionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserCourseProgress_RankProfiles_RankProfileId",
                table: "UserCourseProgress",
                column: "RankProfileId",
                principalTable: "RankProfiles",
                principalColumn: "RankProfileId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrainingAssignments_CourseVersions_ResolvedCourseVersionId",
                table: "UserTrainingAssignments",
                column: "ResolvedCourseVersionId",
                principalTable: "CourseVersions",
                principalColumn: "CourseVersionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrainingAssignments_RankProfiles_ResolvedRankProfileId",
                table: "UserTrainingAssignments",
                column: "ResolvedRankProfileId",
                principalTable: "RankProfiles",
                principalColumn: "RankProfileId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Not reversible. The Up migration deliberately purges the training catalogue and
            // all learner training evidence before restructuring, so there is nothing for a Down
            // migration to restore. Roll back by restoring a database backup taken beforehand.
            throw new NotSupportedException(
                "TrainingModuleReuse cannot be rolled back: it purges training data as part of the " +
                "rebuild. Restore a database backup taken before this migration was applied.");
        }
    }
}

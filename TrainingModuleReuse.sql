BEGIN TRANSACTION;

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
            

ALTER TABLE [Lessons] DROP CONSTRAINT [FK_Lessons_Modules_ModuleId];

ALTER TABLE [TrainingQuestionBankQuestions] DROP CONSTRAINT [FK_TrainingQuestionBankQuestions_Modules_TrainingModuleId];

DROP TABLE [Modules];

EXEC sp_rename N'[TrainingQuestionBankQuestions].[TrainingModuleId]', N'TrainingModuleVersionId', 'COLUMN';

EXEC sp_rename N'[TrainingQuestionBankQuestions].[IX_TrainingQuestionBankQuestions_TrainingModuleId]', N'IX_TrainingQuestionBankQuestions_TrainingModuleVersionId', 'INDEX';

EXEC sp_rename N'[Lessons].[ModuleId]', N'ModuleVersionId', 'COLUMN';

EXEC sp_rename N'[Lessons].[IX_Lessons_ModuleId_OrderIndex]', N'IX_Lessons_ModuleVersionId_OrderIndex', 'INDEX';

CREATE TABLE [TrainingModules] (
    [ModuleId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [Category] nvarchar(100) NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [CurrentVersionId] uniqueidentifier NULL,
    [CreatedByUserId] nvarchar(450) NULL,
    [CreatedOnUtc] datetime2 NOT NULL,
    [UpdatedByUserId] nvarchar(450) NULL,
    [UpdatedOnUtc] datetime2 NULL,
    [IsArchived] bit NOT NULL DEFAULT CAST(0 AS bit),
    [ArchivedByUserId] nvarchar(450) NULL,
    [ArchivedOnUtc] datetime2 NULL,
    [RowVersion] rowversion NULL,
    CONSTRAINT [PK_TrainingModules] PRIMARY KEY ([ModuleId])
);

ALTER TABLE [UserTrainingAssignments] ADD [ResolvedCourseVersionId] uniqueidentifier NULL;

ALTER TABLE [UserTrainingAssignments] ADD [ResolvedOnUtc] datetime2 NULL;

ALTER TABLE [UserTrainingAssignments] ADD [ResolvedRankProfileId] uniqueidentifier NULL;

ALTER TABLE [UserCourseProgress] ADD [CourseVersionId] uniqueidentifier NULL;

ALTER TABLE [UserCourseProgress] ADD [RankProfileId] uniqueidentifier NULL;

ALTER TABLE [TrainingCourseAssessments] ADD [ModuleVersionId] uniqueidentifier NULL;

ALTER TABLE [TrainingCertificates] ADD [CourseCodeSnapshot] nvarchar(50) NULL;

ALTER TABLE [TrainingCertificates] ADD [CourseTitleSnapshot] nvarchar(200) NULL;

ALTER TABLE [TrainingCertificates] ADD [LearnerFullNameSnapshot] nvarchar(200) NULL;

ALTER TABLE [TrainingCertificates] ADD [RankProfileNameSnapshot] nvarchar(150) NULL;

ALTER TABLE [TrainingCertificates] ADD [VersionLabelSnapshot] nvarchar(50) NULL;

ALTER TABLE [CourseVersions] ADD [RowVersion] rowversion NULL;

ALTER TABLE [CourseCompletionRecords] ADD [RankProfileId] uniqueidentifier NULL;

ALTER TABLE [CourseCompletionRecords] ADD [RankProfileName] nvarchar(150) NULL;

CREATE TABLE [ModuleVersions] (
    [ModuleVersionId] uniqueidentifier NOT NULL,
    [ModuleId] uniqueidentifier NOT NULL,
    [VersionNumber] int NOT NULL DEFAULT 1,
    [VersionLabel] nvarchar(50) NULL,
    [Status] int NOT NULL,
    [Title] nvarchar(200) NOT NULL,
    [Description] nvarchar(1000) NULL,
    [EstimatedMinutes] int NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [HasModuleAssessment] bit NOT NULL DEFAULT CAST(0 AS bit),
    [AssessmentId] uniqueidentifier NULL,
    [AssessmentPassMarkPercent] decimal(5,2) NULL,
    [AssessmentMaxAttempts] int NOT NULL DEFAULT 3,
    [ChangeSummary] nvarchar(1000) NULL,
    [PublishedOnUtc] datetime2 NULL,
    [PublishedByUserId] nvarchar(450) NULL,
    [CreatedOnUtc] datetime2 NOT NULL,
    [UpdatedOnUtc] datetime2 NULL,
    [RowVersion] rowversion NULL,
    CONSTRAINT [PK_ModuleVersions] PRIMARY KEY ([ModuleVersionId]),
    CONSTRAINT [FK_ModuleVersions_TrainingCourseAssessments_AssessmentId] FOREIGN KEY ([AssessmentId]) REFERENCES [TrainingCourseAssessments] ([TrainingCourseAssessmentId]),
    CONSTRAINT [FK_ModuleVersions_TrainingModules_ModuleId] FOREIGN KEY ([ModuleId]) REFERENCES [TrainingModules] ([ModuleId]) ON DELETE NO ACTION
);

CREATE TABLE [RankProfiles] (
    [RankProfileId] uniqueidentifier NOT NULL,
    [Code] nvarchar(50) NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Description] nvarchar(500) NULL,
    [OrderIndex] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [IsSystem] bit NOT NULL DEFAULT CAST(0 AS bit),
    [CreatedByUserId] nvarchar(450) NULL,
    [CreatedOnUtc] datetime2 NOT NULL,
    [UpdatedByUserId] nvarchar(450) NULL,
    [UpdatedOnUtc] datetime2 NULL,
    CONSTRAINT [PK_RankProfiles] PRIMARY KEY ([RankProfileId])
);

CREATE TABLE [CourseModules] (
    [CourseModuleId] uniqueidentifier NOT NULL,
    [CourseVersionId] uniqueidentifier NOT NULL,
    [ModuleVersionId] uniqueidentifier NOT NULL,
    [OrderIndex] int NOT NULL,
    [IsRequired] bit NOT NULL DEFAULT CAST(1 AS bit),
    CONSTRAINT [PK_CourseModules] PRIMARY KEY ([CourseModuleId]),
    CONSTRAINT [FK_CourseModules_CourseVersions_CourseVersionId] FOREIGN KEY ([CourseVersionId]) REFERENCES [CourseVersions] ([CourseVersionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_CourseModules_ModuleVersions_ModuleVersionId] FOREIGN KEY ([ModuleVersionId]) REFERENCES [ModuleVersions] ([ModuleVersionId]) ON DELETE NO ACTION
);

CREATE TABLE [CourseRankProfiles] (
    [CourseRankProfileId] uniqueidentifier NOT NULL,
    [CourseVersionId] uniqueidentifier NOT NULL,
    [RankProfileId] uniqueidentifier NOT NULL,
    [OrderIndex] int NOT NULL,
    [IsActive] bit NOT NULL DEFAULT CAST(1 AS bit),
    [TotalDurationMinutes] int NULL,
    CONSTRAINT [PK_CourseRankProfiles] PRIMARY KEY ([CourseRankProfileId]),
    CONSTRAINT [FK_CourseRankProfiles_CourseVersions_CourseVersionId] FOREIGN KEY ([CourseVersionId]) REFERENCES [CourseVersions] ([CourseVersionId]) ON DELETE CASCADE,
    CONSTRAINT [FK_CourseRankProfiles_RankProfiles_RankProfileId] FOREIGN KEY ([RankProfileId]) REFERENCES [RankProfiles] ([RankProfileId]) ON DELETE NO ACTION
);

CREATE TABLE [RankProfileRanks] (
    [RankProfileRankId] uniqueidentifier NOT NULL,
    [RankProfileId] uniqueidentifier NOT NULL,
    [CrewRank] nvarchar(50) NOT NULL,
    CONSTRAINT [PK_RankProfileRanks] PRIMARY KEY ([RankProfileRankId]),
    CONSTRAINT [FK_RankProfileRanks_RankProfiles_RankProfileId] FOREIGN KEY ([RankProfileId]) REFERENCES [RankProfiles] ([RankProfileId]) ON DELETE CASCADE
);

CREATE TABLE [CourseRankModules] (
    [CourseRankModuleId] uniqueidentifier NOT NULL,
    [CourseRankProfileId] uniqueidentifier NOT NULL,
    [CourseModuleId] uniqueidentifier NOT NULL,
    [IsIncluded] bit NOT NULL DEFAULT CAST(1 AS bit),
    [RowVersion] rowversion NULL,
    CONSTRAINT [PK_CourseRankModules] PRIMARY KEY ([CourseRankModuleId]),
    CONSTRAINT [FK_CourseRankModules_CourseModules_CourseModuleId] FOREIGN KEY ([CourseModuleId]) REFERENCES [CourseModules] ([CourseModuleId]),
    CONSTRAINT [FK_CourseRankModules_CourseRankProfiles_CourseRankProfileId] FOREIGN KEY ([CourseRankProfileId]) REFERENCES [CourseRankProfiles] ([CourseRankProfileId]) ON DELETE CASCADE
);

CREATE INDEX [IX_UserTrainingAssignments_ResolvedCourseVersionId] ON [UserTrainingAssignments] ([ResolvedCourseVersionId]);

CREATE INDEX [IX_UserTrainingAssignments_ResolvedRankProfileId] ON [UserTrainingAssignments] ([ResolvedRankProfileId]);

CREATE INDEX [IX_UserCourseProgress_CourseVersionId] ON [UserCourseProgress] ([CourseVersionId]);

CREATE INDEX [IX_UserCourseProgress_RankProfileId] ON [UserCourseProgress] ([RankProfileId]);

CREATE INDEX [IX_TrainingCourseAssessments_ModuleVersionId] ON [TrainingCourseAssessments] ([ModuleVersionId]);

CREATE INDEX [IX_CourseCompletionRecords_RankProfileId] ON [CourseCompletionRecords] ([RankProfileId]);

CREATE INDEX [IX_TrainingModules_Category] ON [TrainingModules] ([Category]);

CREATE UNIQUE INDEX [IX_TrainingModules_Code] ON [TrainingModules] ([Code]);

CREATE INDEX [IX_TrainingModules_IsArchived] ON [TrainingModules] ([IsArchived]);

CREATE UNIQUE INDEX [IX_CourseModules_CourseVersionId_ModuleVersionId] ON [CourseModules] ([CourseVersionId], [ModuleVersionId]);

CREATE INDEX [IX_CourseModules_CourseVersionId_OrderIndex] ON [CourseModules] ([CourseVersionId], [OrderIndex]);

CREATE INDEX [IX_CourseModules_ModuleVersionId] ON [CourseModules] ([ModuleVersionId]);

CREATE INDEX [IX_CourseRankModules_CourseModuleId] ON [CourseRankModules] ([CourseModuleId]);

CREATE UNIQUE INDEX [IX_CourseRankModules_CourseRankProfileId_CourseModuleId] ON [CourseRankModules] ([CourseRankProfileId], [CourseModuleId]);

CREATE INDEX [IX_CourseRankProfiles_CourseVersionId_OrderIndex] ON [CourseRankProfiles] ([CourseVersionId], [OrderIndex]);

CREATE UNIQUE INDEX [IX_CourseRankProfiles_CourseVersionId_RankProfileId] ON [CourseRankProfiles] ([CourseVersionId], [RankProfileId]);

CREATE INDEX [IX_CourseRankProfiles_RankProfileId] ON [CourseRankProfiles] ([RankProfileId]);

CREATE INDEX [IX_ModuleVersions_AssessmentId] ON [ModuleVersions] ([AssessmentId]);

CREATE UNIQUE INDEX [IX_ModuleVersions_ModuleId_VersionNumber] ON [ModuleVersions] ([ModuleId], [VersionNumber]);

CREATE INDEX [IX_ModuleVersions_Status] ON [ModuleVersions] ([Status]);

CREATE INDEX [IX_RankProfileRanks_CrewRank] ON [RankProfileRanks] ([CrewRank]);

CREATE UNIQUE INDEX [IX_RankProfileRanks_RankProfileId_CrewRank] ON [RankProfileRanks] ([RankProfileId], [CrewRank]);

CREATE UNIQUE INDEX [IX_RankProfiles_Code] ON [RankProfiles] ([Code]);

CREATE INDEX [IX_RankProfiles_OrderIndex] ON [RankProfiles] ([OrderIndex]);

ALTER TABLE [CourseCompletionRecords] ADD CONSTRAINT [FK_CourseCompletionRecords_RankProfiles_RankProfileId] FOREIGN KEY ([RankProfileId]) REFERENCES [RankProfiles] ([RankProfileId]) ON DELETE NO ACTION;

ALTER TABLE [Lessons] ADD CONSTRAINT [FK_Lessons_ModuleVersions_ModuleVersionId] FOREIGN KEY ([ModuleVersionId]) REFERENCES [ModuleVersions] ([ModuleVersionId]) ON DELETE CASCADE;

ALTER TABLE [TrainingCourseAssessments] ADD CONSTRAINT [FK_TrainingCourseAssessments_ModuleVersions_ModuleVersionId] FOREIGN KEY ([ModuleVersionId]) REFERENCES [ModuleVersions] ([ModuleVersionId]);

ALTER TABLE [TrainingModules] ADD CONSTRAINT [FK_TrainingModules_ModuleVersions_CurrentVersionId] FOREIGN KEY ([CurrentVersionId]) REFERENCES [ModuleVersions] ([ModuleVersionId]) ON DELETE NO ACTION;

ALTER TABLE [TrainingQuestionBankQuestions] ADD CONSTRAINT [FK_TrainingQuestionBankQuestions_ModuleVersions_TrainingModuleVersionId] FOREIGN KEY ([TrainingModuleVersionId]) REFERENCES [ModuleVersions] ([ModuleVersionId]) ON DELETE SET NULL;

ALTER TABLE [UserCourseProgress] ADD CONSTRAINT [FK_UserCourseProgress_CourseVersions_CourseVersionId] FOREIGN KEY ([CourseVersionId]) REFERENCES [CourseVersions] ([CourseVersionId]) ON DELETE NO ACTION;

ALTER TABLE [UserCourseProgress] ADD CONSTRAINT [FK_UserCourseProgress_RankProfiles_RankProfileId] FOREIGN KEY ([RankProfileId]) REFERENCES [RankProfiles] ([RankProfileId]) ON DELETE NO ACTION;

ALTER TABLE [UserTrainingAssignments] ADD CONSTRAINT [FK_UserTrainingAssignments_CourseVersions_ResolvedCourseVersionId] FOREIGN KEY ([ResolvedCourseVersionId]) REFERENCES [CourseVersions] ([CourseVersionId]) ON DELETE NO ACTION;

ALTER TABLE [UserTrainingAssignments] ADD CONSTRAINT [FK_UserTrainingAssignments_RankProfiles_ResolvedRankProfileId] FOREIGN KEY ([ResolvedRankProfileId]) REFERENCES [RankProfiles] ([RankProfileId]) ON DELETE NO ACTION;

INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260730194124_TrainingModuleReuse', N'10.0.10');

COMMIT;
GO


using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<OperationalArea> OperationalAreas => Set<OperationalArea>();
        public DbSet<Port> Ports => Set<Port>();
        public DbSet<Bay> Bays => Set<Bay>();
        public DbSet<Anchorage> Anchorages => Set<Anchorage>();
        public DbSet<BunkeringCompany> BunkeringCompanies => Set<BunkeringCompany>();
        public DbSet<Vessel> Vessels => Set<Vessel>();
        public DbSet<VesselRoleAssignment> VesselRoleAssignments => Set<VesselRoleAssignment>();
        public DbSet<UserAreaAssignment> UserAreaAssignments => Set<UserAreaAssignment>();
        public DbSet<CompanyAreaAssignment> CompanyAreaAssignments => Set<CompanyAreaAssignment>();
        public DbSet<AuthorityContact> AuthorityContacts => Set<AuthorityContact>();
        public DbSet<AuthorityAreaAssignment> AuthorityAreaAssignments => Set<AuthorityAreaAssignment>();
        public DbSet<AreaNotificationDistributionRule> AreaNotificationDistributionRules => Set<AreaNotificationDistributionRule>();
        public DbSet<CrewDeployment> CrewDeployments => Set<CrewDeployment>();
        public DbSet<CrewDeploymentComplianceSnapshot> CrewDeploymentComplianceSnapshots => Set<CrewDeploymentComplianceSnapshot>();
        public DbSet<VesselCrewListEntry> VesselCrewListEntries => Set<VesselCrewListEntry>();
        public DbSet<CrewMemberDetails> CrewMemberDetails => Set<CrewMemberDetails>();
        public DbSet<CrewChangeHistory> CrewChangeHistory => Set<CrewChangeHistory>();
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<CourseVersion> CourseVersions => Set<CourseVersion>();
        public DbSet<TrainingModule> Modules => Set<TrainingModule>();
        public DbSet<TrainingLesson> Lessons => Set<TrainingLesson>();
        public DbSet<LessonBlock> LessonBlocks => Set<LessonBlock>();
        public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
        public DbSet<Assessment> Assessments => Set<Assessment>();
        public DbSet<AssessmentQuestion> AssessmentQuestions => Set<AssessmentQuestion>();
        public DbSet<AssessmentOption> AssessmentOptions => Set<AssessmentOption>();
        public DbSet<CourseAudienceRule> CourseAudienceRules => Set<CourseAudienceRule>();
        public DbSet<UserTrainingAssignment> UserTrainingAssignments => Set<UserTrainingAssignment>();
        public DbSet<UserLessonProgress> UserLessonProgress => Set<UserLessonProgress>();
        public DbSet<UserCourseProgress> UserCourseProgress => Set<UserCourseProgress>();
        public DbSet<AssessmentAttempt> AssessmentAttempts => Set<AssessmentAttempt>();
        public DbSet<AssessmentResponse> AssessmentResponses => Set<AssessmentResponse>();
        public DbSet<CourseCompletionRecord> CourseCompletionRecords => Set<CourseCompletionRecord>();
        public DbSet<TrainingCertificate> TrainingCertificates => Set<TrainingCertificate>();
        public DbSet<TrainingAuditLog> TrainingAuditLogs => Set<TrainingAuditLog>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(x => x.FullName).HasMaxLength(256);
                entity.Property(x => x.CellNo).HasMaxLength(30);
                entity.Property(x => x.Address).HasMaxLength(300);
                entity.Property(x => x.Country).HasMaxLength(100);
                entity.Property(x => x.RequestedRole).HasMaxLength(64);
                entity.Property(x => x.IsCrew).HasDefaultValue(false);
                entity.Property(x => x.CrewRank)
                    .HasConversion(
                        value => value.HasValue ? value.Value.GetDisplayName() : null,
                        value => CrewRankExtensions.ParseDisplayName(value))
                    .HasMaxLength(50);
                entity.Property(x => x.SidNumber).HasMaxLength(20);
                entity.Property(x => x.SidIssuingCountry).HasMaxLength(100);
                entity.Property(x => x.SidIssuingAuthority).HasMaxLength(150);
                entity.Property(x => x.ProfilePictureContentType).HasMaxLength(100);
                entity.Property(x => x.ApprovalNotes).HasMaxLength(1024);
                entity.Property(x => x.ApprovedByUserId).HasMaxLength(450);
                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.PrimaryArea)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryAreaId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.Vessel)
                    .WithMany()
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<OperationalArea>(entity =>
            {
                entity.HasIndex(x => x.Code).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(500);
            });

            builder.Entity<Port>(entity =>
            {
                entity.HasIndex(x => new { x.OperationalAreaId, x.Code }).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.Ports)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Bay>(entity =>
            {
                entity.HasIndex(x => new { x.OperationalAreaId, x.Code }).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.Bays)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Port)
                    .WithMany(x => x.Bays)
                    .HasForeignKey(x => x.PortId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Anchorage>(entity =>
            {
                entity.HasIndex(x => new { x.OperationalAreaId, x.Code }).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.Anchorages)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Port)
                    .WithMany(x => x.Anchorages)
                    .HasForeignKey(x => x.PortId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(x => x.Bay)
                    .WithMany(x => x.Anchorages)
                    .HasForeignKey(x => x.BayId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<BunkeringCompany>(entity =>
            {
                entity.HasIndex(x => x.RegistrationNumber).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.RegistrationNumber).HasMaxLength(100).IsRequired();
                entity.Property(x => x.ContactEmail).HasMaxLength(256);
                entity.Property(x => x.ContactPhone).HasMaxLength(50);
            });

            builder.Entity<Vessel>(entity =>
            {
                entity.HasIndex(x => x.ImoNumber).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.ImoNumber).HasMaxLength(50).IsRequired();
                entity.Property(x => x.CallSign).HasMaxLength(50);
                entity.Property(x => x.FlagState).HasMaxLength(100);
                entity.HasOne(x => x.Company)
                    .WithMany(x => x.Vessels)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<UserAreaAssignment>(entity =>
            {
                entity.HasIndex(x => new { x.UserId, x.OperationalAreaId }).IsUnique();
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.UserAssignments)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>()
                    .WithMany(x => x.UserAreaAssignments)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CompanyAreaAssignment>(entity =>
            {
                entity.HasIndex(x => new { x.CompanyId, x.OperationalAreaId }).IsUnique();
                entity.HasOne(x => x.Company)
                    .WithMany(x => x.AreaAssignments)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.CompanyAssignments)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AuthorityContact>(entity =>
            {
                entity.Property(x => x.AuthorityRole).HasMaxLength(64).IsRequired();
                entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
                entity.Property(x => x.PhoneNumber).HasMaxLength(50);
                entity.Property(x => x.UserId).HasMaxLength(450);
            });

            builder.Entity<AuthorityAreaAssignment>(entity =>
            {
                entity.HasIndex(x => new { x.AuthorityContactId, x.OperationalAreaId }).IsUnique();
                entity.HasOne(x => x.AuthorityContact)
                    .WithMany(x => x.AreaAssignments)
                    .HasForeignKey(x => x.AuthorityContactId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.AuthorityAssignments)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AreaNotificationDistributionRule>(entity =>
            {
                entity.Property(x => x.NotificationCategory).HasMaxLength(100).IsRequired();
                entity.Property(x => x.RecipientRoleName).HasMaxLength(64).IsRequired();
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.NotificationDistributionRules)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.AuthorityContact)
                    .WithMany(x => x.DistributionRules)
                    .HasForeignKey(x => x.AuthorityContactId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<VesselRoleAssignment>(entity =>
            {
                entity.HasIndex(x => new { x.UserId, x.VesselId, x.VesselRoleType, x.IsActive });
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.AssignedByUserId).HasMaxLength(450);
                entity.HasOne(x => x.Vessel)
                    .WithMany(x => x.RoleAssignments)
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Company)
                    .WithMany(x => x.VesselRoleAssignments)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne<ApplicationUser>()
                    .WithMany(x => x.VesselRoleAssignments)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CrewDeployment>(entity =>
            {
                entity.HasIndex(x => new { x.UserId, x.VesselId, x.Status });
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.OnboardRoleName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.DeployedByUserId).HasMaxLength(450);
                entity.Property(x => x.Notes).HasMaxLength(1000);
                entity.Property(x => x.EmbarkationPort).HasMaxLength(100);
                entity.Property(x => x.DisembarkationPort).HasMaxLength(100);
                entity.Property(x => x.DisembarkationReason).HasMaxLength(200);
                entity.Property(x => x.VaccinationStatus).HasMaxLength(200);
                entity.Property(x => x.Duties).HasMaxLength(200);
                entity.HasOne(x => x.Vessel)
                    .WithMany(x => x.CrewDeployments)
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>()
                    .WithMany(x => x.CrewDeployments)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CrewDeploymentComplianceSnapshot>(entity =>
            {
                entity.Property(x => x.ComplianceItemName).HasMaxLength(200).IsRequired();
                entity.HasOne(x => x.CrewDeployment)
                    .WithMany(x => x.ComplianceSnapshots)
                    .HasForeignKey(x => x.CrewDeploymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<VesselCrewListEntry>(entity =>
            {
                entity.HasIndex(x => new { x.VesselId, x.UserId, x.IsCurrent });
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.OnboardRoleName).HasMaxLength(100).IsRequired();
                entity.HasOne(x => x.Vessel)
                    .WithMany(x => x.CrewListEntries)
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.CrewDeployment)
                    .WithMany(x => x.CrewListEntries)
                    .HasForeignKey(x => x.CrewDeploymentId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne<ApplicationUser>()
                    .WithMany(x => x.VesselCrewListEntries)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CrewMemberDetails>(entity =>
            {
                entity.HasIndex(x => x.UserId).IsUnique();
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.GivenNames).HasMaxLength(150);
                entity.Property(x => x.Gender).HasMaxLength(20);
                entity.Property(x => x.PlaceOfBirth).HasMaxLength(150);
                entity.Property(x => x.Nationality).HasMaxLength(100);
                entity.Property(x => x.PassportNumber).HasMaxLength(50);
                entity.HasOne<ApplicationUser>()
                    .WithOne(x => x.CrewMemberDetails)
                    .HasForeignKey<CrewMemberDetails>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CrewChangeHistory>(entity =>
            {
                entity.Property(x => x.ActionType).HasMaxLength(50).IsRequired();
                entity.Property(x => x.ChangedByUserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.ChangedByName).HasMaxLength(150).IsRequired();
                entity.Property(x => x.ChangedBySurname).HasMaxLength(150).IsRequired();
                entity.Property(x => x.ChangedByRank).HasMaxLength(100).IsRequired();
                entity.Property(x => x.UserId).HasMaxLength(450);
                entity.Property(x => x.Notes).HasMaxLength(500);
                entity.HasOne<Vessel>()
                    .WithMany()
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            ConfigureTrainingEntities(builder);
        }

        private static void ConfigureTrainingEntities(ModelBuilder builder)
        {
            builder.Entity<Course>(entity =>
            {
                entity.ToTable("Courses");
                entity.HasKey(x => x.CourseId);
                entity.HasIndex(x => x.Code).IsUnique();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(2000);
                entity.Property(x => x.ThumbnailUrl).HasMaxLength(500);
                entity.Property(x => x.TargetAudienceSummary).HasMaxLength(500);
                entity.Property(x => x.RegulatoryReference).HasMaxLength(250);
                entity.Property(x => x.ValidityMonths).HasDefaultValue(12);
                entity.Property(x => x.IsMandatory).HasDefaultValue(true);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.Property(x => x.CreatedByUserId).HasMaxLength(450);
                entity.Property(x => x.UpdatedByUserId).HasMaxLength(450);
                entity.HasOne(x => x.CurrentVersion)
                    .WithMany()
                    .HasForeignKey(x => x.CurrentVersionId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<CourseVersion>(entity =>
            {
                entity.ToTable("CourseVersions");
                entity.HasKey(x => x.CourseVersionId);
                entity.HasIndex(x => new { x.CourseId, x.VersionNumber }).IsUnique();
                entity.Property(x => x.VersionLabel).HasMaxLength(50);
                entity.Property(x => x.ChangeSummary).HasMaxLength(1000);
                entity.Property(x => x.ApprovedByUserId).HasMaxLength(450);
                entity.HasOne(x => x.Course)
                    .WithMany(x => x.Versions)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TrainingModule>(entity =>
            {
                entity.ToTable("Modules");
                entity.HasKey(x => x.ModuleId);
                entity.HasIndex(x => new { x.CourseVersionId, x.OrderIndex }).IsUnique();
                entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(1000);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasOne(x => x.CourseVersion)
                    .WithMany(x => x.Modules)
                    .HasForeignKey(x => x.CourseVersionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TrainingLesson>(entity =>
            {
                entity.ToTable("Lessons");
                entity.HasKey(x => x.LessonId);
                entity.HasIndex(x => new { x.ModuleId, x.OrderIndex }).IsUnique();
                entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Summary).HasMaxLength(1000);
                entity.Property(x => x.IsRequired).HasDefaultValue(true);
                entity.Property(x => x.IsPreview).HasDefaultValue(false);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasOne(x => x.Module)
                    .WithMany(x => x.Lessons)
                    .HasForeignKey(x => x.ModuleId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<LessonBlock>(entity =>
            {
                entity.ToTable("LessonBlocks");
                entity.HasKey(x => x.LessonBlockId);
                entity.HasIndex(x => new { x.LessonId, x.OrderIndex }).IsUnique();
                entity.Property(x => x.Title).HasMaxLength(200);
                entity.Property(x => x.FileUrl).HasMaxLength(1000);
                entity.Property(x => x.ExternalUrl).HasMaxLength(1000);
                entity.Property(x => x.MimeType).HasMaxLength(100);
                entity.Property(x => x.IsRequired).HasDefaultValue(true);
                entity.HasOne(x => x.Lesson)
                    .WithMany(x => x.LessonBlocks)
                    .HasForeignKey(x => x.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.MediaAsset)
                    .WithMany(x => x.LessonBlocks)
                    .HasForeignKey(x => x.MediaAssetId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<MediaAsset>(entity =>
            {
                entity.ToTable("MediaAssets");
                entity.HasKey(x => x.MediaAssetId);
                entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
                entity.Property(x => x.StoredFileName).HasMaxLength(255).IsRequired();
                entity.Property(x => x.RelativePath).HasMaxLength(500).IsRequired();
                entity.Property(x => x.ContentType).HasMaxLength(100).IsRequired();
                entity.Property(x => x.UploadedByUserId).HasMaxLength(450);
                entity.Property(x => x.HashSha256).HasMaxLength(128);
            });

            builder.Entity<Assessment>(entity =>
            {
                entity.ToTable("Assessments");
                entity.HasKey(x => x.AssessmentId);
                entity.HasIndex(x => x.LessonId).IsUnique();
                entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
                entity.Property(x => x.PassMarkPercent).HasPrecision(5, 2).HasDefaultValue(80m);
                entity.Property(x => x.MaxAttempts).HasDefaultValue(3);
                entity.Property(x => x.ShowFeedbackAfterSubmit).HasDefaultValue(true);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasOne(x => x.Lesson)
                    .WithOne(x => x.Assessment)
                    .HasForeignKey<Assessment>(x => x.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AssessmentQuestion>(entity =>
            {
                entity.ToTable("AssessmentQuestions");
                entity.HasKey(x => x.AssessmentQuestionId);
                entity.HasIndex(x => new { x.AssessmentId, x.OrderIndex });
                entity.Property(x => x.PromptMarkdown).IsRequired();
                entity.Property(x => x.Points).HasPrecision(8, 2).HasDefaultValue(1m);
                entity.HasOne(x => x.Assessment)
                    .WithMany(x => x.Questions)
                    .HasForeignKey(x => x.AssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AssessmentOption>(entity =>
            {
                entity.ToTable("AssessmentOptions");
                entity.HasKey(x => x.AssessmentOptionId);
                entity.HasIndex(x => new { x.AssessmentQuestionId, x.OrderIndex });
                entity.Property(x => x.OptionText).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.IsCorrect).HasDefaultValue(false);
                entity.HasOne(x => x.AssessmentQuestion)
                    .WithMany(x => x.Options)
                    .HasForeignKey(x => x.AssessmentQuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CourseAudienceRule>(entity =>
            {
                entity.ToTable("CourseAudienceRules");
                entity.HasKey(x => x.CourseAudienceRuleId);
                entity.Property(x => x.ApplicationRoleName).HasMaxLength(100);
                entity.Property(x => x.OnBoardRole).HasMaxLength(100);
                entity.Property(x => x.Qualification).HasMaxLength(150);
                entity.Property(x => x.Notes).HasMaxLength(500);
                entity.Property(x => x.IsMandatory).HasDefaultValue(true);
                entity.HasOne(x => x.Course)
                    .WithMany(x => x.AudienceRules)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserTrainingAssignment>(entity =>
            {
                entity.ToTable("UserTrainingAssignments");
                entity.HasKey(x => x.UserTrainingAssignmentId);
                entity.HasIndex(x => new { x.UserId, x.CourseId }).IsUnique();
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.AssignedByUserId).HasMaxLength(450);
                entity.Property(x => x.Reason).HasMaxLength(500);
                entity.HasOne(x => x.Course)
                    .WithMany(x => x.UserTrainingAssignments)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserLessonProgress>(entity =>
            {
                entity.ToTable("UserLessonProgress");
                entity.HasKey(x => x.UserLessonProgressId);
                entity.HasIndex(x => new { x.UserId, x.LessonId }).IsUnique();
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.PercentComplete).HasPrecision(5, 2).HasDefaultValue(0m);
                entity.Property(x => x.ScrollPercent).HasPrecision(5, 2);
                entity.HasOne(x => x.Lesson)
                    .WithMany(x => x.UserLessonProgressRecords)
                    .HasForeignKey(x => x.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserCourseProgress>(entity =>
            {
                entity.ToTable("UserCourseProgress");
                entity.HasKey(x => x.UserCourseProgressId);
                entity.HasIndex(x => new { x.UserId, x.CourseId }).IsUnique();
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.PercentComplete).HasPrecision(5, 2).HasDefaultValue(0m);
                entity.HasOne(x => x.Course)
                    .WithMany(x => x.UserCourseProgressRecords)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.CurrentLesson)
                    .WithMany(x => x.CurrentCourseProgressRecords)
                    .HasForeignKey(x => x.CurrentLessonId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AssessmentAttempt>(entity =>
            {
                entity.ToTable("AssessmentAttempts");
                entity.HasKey(x => x.AssessmentAttemptId);
                entity.HasIndex(x => new { x.AssessmentId, x.UserId, x.AttemptNumber }).IsUnique();
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.ScorePercent).HasPrecision(5, 2);
                entity.Property(x => x.Passed).HasDefaultValue(false);
                entity.HasOne(x => x.Assessment)
                    .WithMany(x => x.Attempts)
                    .HasForeignKey(x => x.AssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AssessmentResponse>(entity =>
            {
                entity.ToTable("AssessmentResponses");
                entity.HasKey(x => x.AssessmentResponseId);
                entity.Property(x => x.AwardedPoints).HasPrecision(8, 2);
                entity.HasOne(x => x.AssessmentAttempt)
                    .WithMany(x => x.Responses)
                    .HasForeignKey(x => x.AssessmentAttemptId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.AssessmentQuestion)
                    .WithMany(x => x.Responses)
                    .HasForeignKey(x => x.AssessmentQuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.SelectedOption)
                    .WithMany(x => x.Responses)
                    .HasForeignKey(x => x.SelectedOptionId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<CourseCompletionRecord>(entity =>
            {
                entity.ToTable("CourseCompletionRecords");
                entity.HasKey(x => x.CourseCompletionRecordId);
                entity.HasIndex(x => x.CertificateNumber).IsUnique();
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.CertificateNumber).HasMaxLength(100).IsRequired();
                entity.Property(x => x.FinalScorePercent).HasPrecision(5, 2);
                entity.HasOne(x => x.Course)
                    .WithMany(x => x.CompletionRecords)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.CourseVersion)
                    .WithMany(x => x.CompletionRecords)
                    .HasForeignKey(x => x.CourseVersionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TrainingCertificate>(entity =>
            {
                entity.ToTable("TrainingCertificates");
                entity.HasKey(x => x.TrainingCertificateId);
                entity.HasIndex(x => x.CourseCompletionRecordId).IsUnique();
                entity.HasIndex(x => x.CertificateNumber).IsUnique();
                entity.HasIndex(x => x.VerificationCode).IsUnique();
                entity.Property(x => x.CertificateNumber).HasMaxLength(100).IsRequired();
                entity.Property(x => x.FilePath).HasMaxLength(500);
                entity.Property(x => x.VerificationCode).HasMaxLength(100).IsRequired();
                entity.HasOne(x => x.CourseCompletionRecord)
                    .WithOne(x => x.TrainingCertificate)
                    .HasForeignKey<TrainingCertificate>(x => x.CourseCompletionRecordId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<TrainingAuditLog>(entity =>
            {
                entity.ToTable("TrainingAuditLogs");
                entity.HasKey(x => x.TrainingAuditLogId);
                entity.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.EntityId).HasMaxLength(150).IsRequired();
                entity.Property(x => x.ActionType).HasMaxLength(50).IsRequired();
                entity.Property(x => x.ChangedByUserId).HasMaxLength(450);
                entity.Property(x => x.Notes).HasMaxLength(500);
            });
        }
    }
}

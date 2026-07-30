using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Models.Authoring;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<Course> Courses => Set<Course>();
        public DbSet<CourseVersion> CourseVersions => Set<CourseVersion>();
        public DbSet<TrainingModule> Modules => Set<TrainingModule>();
        public DbSet<TrainingLesson> Lessons => Set<TrainingLesson>();
        public DbSet<LessonBlock> LessonBlocks => Set<LessonBlock>();
        public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();
        public DbSet<CourseAudienceRule> CourseAudienceRules => Set<CourseAudienceRule>();
        public DbSet<UserTrainingAssignment> UserTrainingAssignments => Set<UserTrainingAssignment>();
        public DbSet<UserLessonProgress> UserLessonProgress => Set<UserLessonProgress>();
        public DbSet<UserCourseProgress> UserCourseProgress => Set<UserCourseProgress>();
        public DbSet<CourseCompletionRecord> CourseCompletionRecords => Set<CourseCompletionRecord>();
        public DbSet<TrainingCertificate> TrainingCertificates => Set<TrainingCertificate>();
        public DbSet<TrainingAuditLog> TrainingAuditLogs => Set<TrainingAuditLog>();
        public DbSet<TrainingCourseAssessment> TrainingCourseAssessments => Set<TrainingCourseAssessment>();
        public DbSet<TrainingQuestionBankQuestion> TrainingQuestionBankQuestions => Set<TrainingQuestionBankQuestion>();
        public DbSet<TrainingQuestionBankOption> TrainingQuestionBankOptions => Set<TrainingQuestionBankOption>();
        public DbSet<UserAssessmentAttempt> UserAssessmentAttempts => Set<UserAssessmentAttempt>();
        public DbSet<UserAssessmentResponse> UserAssessmentResponses => Set<UserAssessmentResponse>();
        public DbSet<Lesson> AuthoringLessons => Set<Lesson>();
        public DbSet<LessonContentItem> LessonContentItems => Set<LessonContentItem>();
        public DbSet<AreaOfOperation> AreasOfOperation => Set<AreaOfOperation>();
        public DbSet<Port> Ports => Set<Port>();
        public DbSet<BunkerOperator> BunkerOperators => Set<BunkerOperator>();
        public DbSet<OperatorAreaAssignment> OperatorAreaAssignments => Set<OperatorAreaAssignment>();
        public DbSet<BunkerBarge> BunkerBarges => Set<BunkerBarge>();
        public DbSet<BargeDeployment> BargeDeployments => Set<BargeDeployment>();
        public DbSet<BargeDeploymentAudit> BargeDeploymentAudits => Set<BargeDeploymentAudit>();

        // Crewing module
        public DbSet<Vessel> Vessels => Set<Vessel>();
        public DbSet<CrewMember> CrewMembers => Set<CrewMember>();
        public DbSet<CrewDocument> CrewDocuments => Set<CrewDocument>();
        public DbSet<CrewAssignment> CrewAssignments => Set<CrewAssignment>();
        public DbSet<VesselComplianceSnapshot> VesselComplianceSnapshots => Set<VesselComplianceSnapshot>();
        public DbSet<ComplianceRule> ComplianceRules => Set<ComplianceRule>();
        public DbSet<ComplianceResult> ComplianceResults => Set<ComplianceResult>();
        public DbSet<CertificateExpiryEvent> CertificateExpiryEvents => Set<CertificateExpiryEvent>();
        public DbSet<ComplianceAuditEntry> ComplianceAuditEntries => Set<ComplianceAuditEntry>();

        // Billing & notifications
        public DbSet<OperatorBillingAccount> OperatorBillingAccounts => Set<OperatorBillingAccount>();
        public DbSet<Invoice> Invoices => Set<Invoice>();
        public DbSet<InvoiceLineItem> InvoiceLineItems => Set<InvoiceLineItem>();
        public DbSet<NotificationMessage> NotificationMessages => Set<NotificationMessage>();

        // Bunkering operations
        public DbSet<BunkerFuel> BunkerFuels => Set<BunkerFuel>();
        public DbSet<BunkeringOperation> BunkeringOperations => Set<BunkeringOperation>();
        public DbSet<BunkeringOperationPumpingInterval> BunkeringOperationPumpingIntervals => Set<BunkeringOperationPumpingInterval>();
        public DbSet<ISGOTTStageTemplate> ISGOTTStageTemplates => Set<ISGOTTStageTemplate>();
        public DbSet<ISGOTTItemTemplate> ISGOTTItemTemplates => Set<ISGOTTItemTemplate>();
        public DbSet<ISGOTTChecklistStageResponse> ISGOTTChecklistStageResponses => Set<ISGOTTChecklistStageResponse>();
        public DbSet<ISGOTTChecklistItemResponse> ISGOTTChecklistItemResponses => Set<ISGOTTChecklistItemResponse>();

        // Emergency Management - Oil Spill Modelling & Response
        public DbSet<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillIncident> OilSpillIncidents => Set<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillIncident>();
        public DbSet<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillModelRun> OilSpillModelRuns => Set<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillModelRun>();
        public DbSet<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillTrajectoryPoint> OilSpillTrajectoryPoints => Set<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillTrajectoryPoint>();
        public DbSet<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillResponseAction> OilSpillResponseActions => Set<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillResponseAction>();
        public DbSet<AlgoaBayBMT.Emergency.OilSpill.Models.IncidentForm> IncidentForms => Set<AlgoaBayBMT.Emergency.OilSpill.Models.IncidentForm>();

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
                entity.Property(x => x.IsCrewManager).HasDefaultValue(false);
                entity.Property(x => x.IsBunkerManager).HasDefaultValue(false);
                entity.HasOne<BunkerOperator>()
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<Lesson>(entity =>
            {
                entity.ToTable("AuthoringLessons");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(1000);
            });

            builder.Entity<LessonContentItem>(entity =>
            {
                entity.ToTable("LessonContentItems");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.LessonId, x.ContentType });
                entity.Property(x => x.ContentType).HasMaxLength(32).IsRequired();
                entity.Property(x => x.Title).HasMaxLength(200);
                entity.Property(x => x.FrontText).HasMaxLength(500);
                entity.Property(x => x.VideoUrl).HasMaxLength(1000);
                entity.Property(x => x.OptionA).HasMaxLength(500);
                entity.Property(x => x.OptionB).HasMaxLength(500);
                entity.Property(x => x.OptionC).HasMaxLength(500);
                entity.Property(x => x.OptionD).HasMaxLength(500);
                entity.Property(x => x.Option1).HasMaxLength(500);
                entity.Property(x => x.Option2).HasMaxLength(500);
                entity.Property(x => x.Option3).HasMaxLength(500);
                entity.Property(x => x.Option4).HasMaxLength(500);
                entity.Property(x => x.CorrectOption).HasMaxLength(50);
                entity.HasOne(x => x.Lesson)
                    .WithMany(x => x.ContentItems)
                    .HasForeignKey(x => x.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            ConfigureTrainingEntities(builder);
            ConfigureBunkerOperationEntities(builder);
            ConfigureBunkeringModuleEntities(builder);
            ConfigureCrewingEntities(builder);
            ConfigureBillingEntities(builder);
            ConfigureOilSpillEntities(builder);
        }

        private static void ConfigureOilSpillEntities(ModelBuilder builder)
        {
            builder.Entity<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillIncident>(entity =>
            {
                entity.ToTable("OilSpillIncidents");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.SpillName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
                entity.HasIndex(x => x.Status);
                entity.HasIndex(x => x.BunkeringOperationId);
                entity.HasOne(x => x.Operation)
                    .WithMany()
                    .HasForeignKey(x => x.BunkeringOperationId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(x => x.ModelRuns)
                    .WithOne(x => x.Spill)
                    .HasForeignKey(x => x.SpillId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.ResponseActions)
                    .WithOne(x => x.Spill)
                    .HasForeignKey(x => x.SpillId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillModelRun>(entity =>
            {
                entity.ToTable("OilSpillModelRuns");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.RunName).HasMaxLength(200).IsRequired();
                entity.HasIndex(x => x.SpillId);
                entity.HasMany(x => x.TrajectoryPoints)
                    .WithOne(x => x.ModelRun)
                    .HasForeignKey(x => x.ModelRunId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillTrajectoryPoint>(entity =>
            {
                entity.ToTable("OilSpillTrajectoryPoints");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.ModelRunId, x.Timestamp });
            });

            builder.Entity<AlgoaBayBMT.Emergency.OilSpill.Models.OilSpillResponseAction>(entity =>
            {
                entity.ToTable("OilSpillResponseActions");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Description).HasMaxLength(500).IsRequired();
                entity.Property(x => x.PerformedBy).HasMaxLength(100).IsRequired();
                entity.HasIndex(x => x.SpillId);
            });

            builder.Entity<AlgoaBayBMT.Emergency.OilSpill.Models.IncidentForm>(entity =>
            {
                entity.ToTable("IncidentForms");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.CreatedBy).HasMaxLength(100).IsRequired();
                entity.Property(x => x.PersonName).HasMaxLength(150);
                entity.Property(x => x.ExportFilePath).HasMaxLength(400);
                entity.HasIndex(x => x.IncidentId);
                entity.HasIndex(x => new { x.IncidentId, x.FormType });
                entity.HasIndex(x => new { x.IncidentId, x.FormType, x.OperationalPeriodId });
                entity.HasOne(x => x.Spill)
                    .WithMany(x => x.Forms)
                    .HasForeignKey(x => x.IncidentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }

        private static void ConfigureBunkeringModuleEntities(ModelBuilder builder)
        {
            builder.Entity<BunkerFuel>(entity =>
            {
                entity.ToTable("BunkerFuels");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Code).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(120).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(40).IsRequired();
            });

            builder.Entity<BunkeringOperation>(entity =>
            {
                entity.ToTable("BunkeringOperations");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.BunkerVesselId);
                entity.HasIndex(x => x.CustomerVesselId);
                entity.Property(x => x.TotalQuantity).HasPrecision(18, 3);
                entity.Property(x => x.CreatedByUserId).HasMaxLength(450).IsRequired();
                entity.HasOne(x => x.BunkerVessel).WithMany().HasForeignKey(x => x.BunkerVesselId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.CustomerVessel).WithMany().HasForeignKey(x => x.CustomerVesselId).OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.BunkerFuel).WithMany().HasForeignKey(x => x.BunkerFuelId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(x => x.PumpingIntervals).WithOne(x => x.BunkeringOperation).HasForeignKey(x => x.BunkeringOperationId).OnDelete(DeleteBehavior.Cascade);
                entity.HasMany(x => x.ISGOTTStageResponses).WithOne(x => x.BunkeringOperation).HasForeignKey(x => x.BunkeringOperationId).OnDelete(DeleteBehavior.Cascade);
                // Vessel has a global soft-delete query filter; mirror it here since both vessel
                // navigations are required, so a deleted vessel doesn't leave a dangling required reference.
                entity.HasQueryFilter(x => !x.BunkerVessel!.IsDeleted && !x.CustomerVessel!.IsDeleted);
            });

            builder.Entity<BunkeringOperationPumpingInterval>(entity =>
            {
                entity.ToTable("BunkeringOperationPumpingIntervals");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Quantity).HasPrecision(18, 3);
                entity.Property(x => x.WindSpeed).HasMaxLength(80);
                entity.Property(x => x.WindDirection).HasMaxLength(80);
                entity.Property(x => x.SeaState).HasMaxLength(120);
                entity.Property(x => x.Swell).HasMaxLength(120);
                entity.Property(x => x.Visibility).HasMaxLength(120);
                entity.Property(x => x.Notes).HasMaxLength(1000);
                entity.HasOne(x => x.BunkerFuel).WithMany().HasForeignKey(x => x.BunkerFuelId).OnDelete(DeleteBehavior.Restrict);
                entity.HasQueryFilter(x => !x.BunkeringOperation!.BunkerVessel!.IsDeleted && !x.BunkeringOperation!.CustomerVessel!.IsDeleted);
            });

            builder.Entity<ISGOTTStageTemplate>(entity =>
            {
                entity.ToTable("ISGOTTStageTemplates");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasMany(x => x.Items).WithOne(x => x.StageTemplate).HasForeignKey(x => x.StageTemplateId).OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<ISGOTTItemTemplate>(entity =>
            {
                entity.ToTable("ISGOTTItemTemplates");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Text).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.ItemType).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
            });

            builder.Entity<ISGOTTChecklistStageResponse>(entity =>
            {
                entity.ToTable("ISGOTTChecklistStageResponses");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.CompletedByUserId).HasMaxLength(450).IsRequired();
                entity.HasOne(x => x.StageTemplate).WithMany().HasForeignKey(x => x.StageTemplateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(x => x.ItemResponses).WithOne(x => x.StageResponse).HasForeignKey(x => x.StageResponseId).OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(x => !x.BunkeringOperation!.BunkerVessel!.IsDeleted && !x.BunkeringOperation!.CustomerVessel!.IsDeleted);
            });

            builder.Entity<ISGOTTChecklistItemResponse>(entity =>
            {
                entity.ToTable("ISGOTTChecklistItemResponses");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.YesNoNaValue).HasConversion<string>().HasMaxLength(10);
                entity.Property(x => x.TextValue).HasMaxLength(2000);
                entity.Property(x => x.NumericValue).HasPrecision(18, 3);
                entity.HasOne(x => x.ItemTemplate).WithMany().HasForeignKey(x => x.ItemTemplateId).OnDelete(DeleteBehavior.Restrict);
                entity.HasQueryFilter(x => !x.StageResponse!.BunkeringOperation!.BunkerVessel!.IsDeleted && !x.StageResponse!.BunkeringOperation!.CustomerVessel!.IsDeleted);
            });
        }

        private static void ConfigureBunkerOperationEntities(ModelBuilder builder)
        {
            builder.Entity<AreaOfOperation>(entity =>
            {
                entity.ToTable("BunkerAreasOfOperation");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Name).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(1000);
                entity.Property(x => x.RegionCode).HasMaxLength(50);
                entity.Property(x => x.EnvironmentalSensitivityRating).HasMaxLength(100);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
            });

            builder.Entity<Port>(entity =>
            {
                entity.ToTable("BunkerPorts");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.AreaOfOperationId);
                entity.HasIndex(x => new { x.AreaOfOperationId, x.Name });
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Type).HasMaxLength(20).IsRequired();
                entity.Property(x => x.MaxVesselSize).HasMaxLength(100);
                entity.Property(x => x.IsBunkeringAllowed).HasDefaultValue(true);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasOne(x => x.AreaOfOperation)
                    .WithMany(x => x.Ports)
                    .HasForeignKey(x => x.AreaOfOperationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<BunkerOperator>(entity =>
            {
                entity.ToTable("BunkerOperators");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.Name).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.CompanyRegistrationNumber).HasMaxLength(100);
                entity.Property(x => x.PhysicalAddress).HasMaxLength(500);
                entity.Property(x => x.ContactPerson).HasMaxLength(150);
                entity.Property(x => x.Email).HasMaxLength(254);
                entity.Property(x => x.Phone).HasMaxLength(50);
                entity.Property(x => x.EmergencyContactNumber).HasMaxLength(50);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
            });

            builder.Entity<OperatorAreaAssignment>(entity =>
            {
                entity.ToTable("BunkerOperatorAreaAssignments");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.BunkerOperatorId, x.AreaOfOperationId, x.AssignedFrom, x.AssignedTo });
                entity.HasOne(x => x.BunkerOperator)
                    .WithMany(x => x.AreaAssignments)
                    .HasForeignKey(x => x.BunkerOperatorId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.AreaOfOperation)
                    .WithMany(x => x.OperatorAssignments)
                    .HasForeignKey(x => x.AreaOfOperationId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<BunkerBarge>(entity =>
            {
                entity.ToTable("BunkerBarges");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.BunkerOperatorId);
                entity.HasIndex(x => x.Name);
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.IMO).HasMaxLength(20);
                entity.Property(x => x.MMSI).HasMaxLength(20);
                entity.Property(x => x.CallSign).HasMaxLength(30);
                entity.Property(x => x.FuelTypesSupported).HasMaxLength(300);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasOne(x => x.BunkerOperator)
                    .WithMany(x => x.Barges)
                    .HasForeignKey(x => x.BunkerOperatorId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<BargeDeployment>(entity =>
            {
                entity.ToTable("BunkerBargeDeployments");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.BunkerBargeId, x.DeployedFrom, x.DeployedTo });
                entity.HasIndex(x => new { x.AreaOfOperationId, x.DeployedFrom, x.DeployedTo });
                entity.Property(x => x.Notes).HasMaxLength(1000);
                entity.HasOne(x => x.BunkerBarge)
                    .WithMany(x => x.Deployments)
                    .HasForeignKey(x => x.BunkerBargeId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.AreaOfOperation)
                    .WithMany(x => x.BargeDeployments)
                    .HasForeignKey(x => x.AreaOfOperationId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            builder.Entity<BargeDeploymentAudit>(entity =>
            {
                entity.ToTable("BunkerBargeDeploymentAudits");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.BargeDeploymentId, x.Timestamp });
                entity.Property(x => x.Action).HasMaxLength(50).IsRequired();
                entity.Property(x => x.PerformedBy).HasMaxLength(256).IsRequired();
                entity.Property(x => x.Details).HasMaxLength(1000).IsRequired();
                entity.HasOne(x => x.Deployment)
                    .WithMany(x => x.AuditEntries)
                    .HasForeignKey(x => x.BargeDeploymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
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
                entity.Property(x => x.Summary).HasMaxLength(1000);
                entity.Property(x => x.Description).HasMaxLength(2000);
                entity.Property(x => x.PassMarkPercent).HasPrecision(5, 2).HasDefaultValue(80m);
                entity.Property(x => x.ThumbnailUrl).HasMaxLength(500);
                entity.Property(x => x.TargetAudienceSummary).HasMaxLength(500);
                entity.Property(x => x.RegulatoryReference).HasMaxLength(250);
                entity.Property(x => x.Cost).HasPrecision(18, 2).HasDefaultValue(0m);
                entity.Property(x => x.ValidityMonths).HasDefaultValue(12);
                entity.Property(x => x.IsMandatory).HasDefaultValue(true);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.Property(x => x.IsDeleted).HasDefaultValue(false);
                entity.Property(x => x.DeletedByUserId).HasMaxLength(450);
                entity.Property(x => x.CreatedByUserId).HasMaxLength(450);
                entity.Property(x => x.UpdatedByUserId).HasMaxLength(450);
                entity.HasIndex(x => x.IsDeleted);
                entity.HasOne(x => x.CurrentVersion)
                    .WithMany()
                    .HasForeignKey(x => x.CurrentVersionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasMany(x => x.Assessments)
                    .WithOne(x => x.Course)
                    .HasForeignKey(x => x.TrainingCourseId)
                    .OnDelete(DeleteBehavior.NoAction);
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
                // Non-unique: ordering is compacted by the service's reindex logic, and a
                // unique index makes swap-based reordering throw transient constraint violations.
                entity.HasIndex(x => new { x.CourseVersionId, x.OrderIndex });
                entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(1000);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.Property(x => x.HasModuleAssessment).HasDefaultValue(false);
                entity.Property(x => x.AssessmentPassMarkPercent).HasPrecision(5, 2);
                entity.Property(x => x.AssessmentMaxAttempts).HasDefaultValue(3);
                entity.HasOne(x => x.CourseVersion)
                    .WithMany(x => x.Modules)
                    .HasForeignKey(x => x.CourseVersionId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.ModuleAssessment)
                    .WithMany()
                    .HasForeignKey(x => x.AssessmentId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<TrainingLesson>(entity =>
            {
                entity.ToTable("Lessons");
                entity.HasKey(x => x.LessonId);
                entity.HasIndex(x => new { x.ModuleId, x.OrderIndex });
                entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Summary).HasMaxLength(1000);
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
                entity.HasIndex(x => new { x.LessonId, x.OrderIndex });
                entity.Property(x => x.Title).HasMaxLength(200);
                entity.Property(x => x.Subtitle).HasMaxLength(300);
                entity.Property(x => x.ThumbnailUrl).HasMaxLength(500);
                entity.Property(x => x.FileUrl).HasMaxLength(1000);
                entity.Property(x => x.ExternalUrl).HasMaxLength(1000);
                entity.Property(x => x.MimeType).HasMaxLength(100);
                entity.Property(x => x.IsRequired).HasDefaultValue(true);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasOne(x => x.Lesson)
                    .WithMany(x => x.LessonBlocks)
                    .HasForeignKey(x => x.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.MediaAsset)
                    .WithMany(x => x.LessonBlocks)
                    .HasForeignKey(x => x.MediaAssetId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<TrainingCourseAssessment>(entity =>
            {
                entity.ToTable("TrainingCourseAssessments");
                entity.HasKey(x => x.TrainingCourseAssessmentId);
                entity.HasIndex(x => x.TrainingCourseId);
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.PassMarkPercent).HasPrecision(5, 2).HasDefaultValue(80m);
                entity.Property(x => x.RandomQuestionCount).HasDefaultValue(25);
                entity.Property(x => x.MaxAttempts).HasDefaultValue(3);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
            });

            builder.Entity<TrainingQuestionBankQuestion>(entity =>
            {
                entity.ToTable("TrainingQuestionBankQuestions");
                entity.HasKey(x => x.TrainingQuestionBankQuestionId);
                entity.Property(x => x.Prompt).IsRequired();
                entity.Property(x => x.Points).HasPrecision(8, 2).HasDefaultValue(1m);
                entity.Property(x => x.IsActive).HasDefaultValue(true);
                entity.HasOne(x => x.Assessment)
                    .WithMany(x => x.QuestionBankQuestions)
                    .HasForeignKey(x => x.TrainingCourseAssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Module)
                    .WithMany(x => x.QuestionBankQuestions)
                    .HasForeignKey(x => x.TrainingModuleId)
                    .OnDelete(DeleteBehavior.SetNull);
                // Lesson-scoped question bank: TrainingLessonId is a soft reference (indexed, no FK)
                // so the end-of-lesson quiz can draw a random subset specific to that lesson. A hard
                // FK to Lessons would introduce multiple cascade paths (SQL Server error 1785) because
                // the question already reaches Lessons via Assessment -> Course -> Version -> Module.
                entity.HasIndex(x => x.TrainingLessonId);
            });

            builder.Entity<TrainingQuestionBankOption>(entity =>
            {
                entity.ToTable("TrainingQuestionBankOptions");
                entity.HasKey(x => x.TrainingQuestionBankOptionId);
                entity.HasIndex(x => new { x.TrainingQuestionBankQuestionId, x.OrderIndex }).IsUnique();
                entity.Property(x => x.OptionText).HasMaxLength(1000).IsRequired();
                entity.Property(x => x.IsCorrect).HasDefaultValue(false);
                entity.HasOne(x => x.Question)
                    .WithMany(x => x.Options)
                    .HasForeignKey(x => x.TrainingQuestionBankQuestionId)
                    .OnDelete(DeleteBehavior.Cascade);
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
                entity.HasIndex(x => x.Status);
                entity.HasIndex(x => new { x.UserId, x.Status });
                entity.HasIndex(x => x.InvoiceId);
                entity.HasIndex(x => x.ApprovedOnUtc);
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.AssignedByUserId).HasMaxLength(450);
                entity.Property(x => x.Reason).HasMaxLength(500);
                entity.Property(x => x.RegisteredByUserId).HasMaxLength(450);
                entity.Property(x => x.ApprovedByUserId).HasMaxLength(450);
                entity.Property(x => x.ApprovalNotes).HasMaxLength(1000);
                entity.Property(x => x.CompletionScorePercent).HasPrecision(5, 2);
                entity.Property(x => x.PaidByUserId).HasMaxLength(450);
                entity.HasOne(x => x.Course)
                    .WithMany(x => x.UserTrainingAssignments)
                    .HasForeignKey(x => x.CourseId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Invoice)
                    .WithMany()
                    .HasForeignKey(x => x.InvoiceId)
                    .OnDelete(DeleteBehavior.SetNull);
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
                entity.Property(x => x.KnowledgeCheckPassed).HasDefaultValue(false);
                entity.HasOne(x => x.Lesson)
                    .WithMany(x => x.UserLessonProgressRecords)
                    .HasForeignKey(x => x.LessonId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserAssessmentAttempt>(entity =>
            {
                entity.ToTable("UserAssessmentAttempts");
                entity.HasKey(x => x.UserAssessmentAttemptId);
                entity.HasIndex(x => new { x.TrainingCourseAssessmentId, x.UserId, x.AttemptNumber }).IsUnique();
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.ScorePercent).HasPrecision(5, 2);
                entity.Property(x => x.Passed).HasDefaultValue(false);
                entity.HasOne(x => x.Assessment)
                    .WithMany(x => x.Attempts)
                    .HasForeignKey(x => x.TrainingCourseAssessmentId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<UserAssessmentResponse>(entity =>
            {
                entity.ToTable("UserAssessmentResponses");
                entity.HasKey(x => x.UserAssessmentResponseId);
                entity.Property(x => x.AwardedPoints).HasPrecision(8, 2);
                entity.HasOne(x => x.Attempt)
                    .WithMany(x => x.Responses)
                    .HasForeignKey(x => x.UserAssessmentAttemptId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Question)
                    .WithMany(x => x.Responses)
                    .HasForeignKey(x => x.TrainingQuestionBankQuestionId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.SelectedOption)
                    .WithMany(x => x.Responses)
                    .HasForeignKey(x => x.SelectedOptionId)
                    .OnDelete(DeleteBehavior.NoAction);
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

        private static void ConfigureCrewingEntities(ModelBuilder builder)
        {
            builder.Entity<Vessel>(entity =>
            {
                entity.ToTable("Vessels");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.IMO).IsUnique().HasFilter("[IMO] IS NOT NULL");
                entity.HasIndex(x => x.Name);
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Beam).HasPrecision(18, 2);
                entity.Property(x => x.LengthOverall).HasPrecision(18, 2);
                entity.HasOne(x => x.OwningOperator)
                    .WithMany()
                    .HasForeignKey(x => x.OwningOperatorId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasQueryFilter(x => !x.IsDeleted);
            });

            builder.Entity<CrewMember>(entity =>
            {
                entity.ToTable("CrewMembers");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.ApplicationUserId);
                entity.HasIndex(x => new { x.LastName, x.FirstName });
                entity.HasIndex(x => x.SidNumber);
                entity.HasIndex(x => x.PassportNumber);
                entity.Property(x => x.Rank)
                    .HasConversion(
                        value => value.HasValue ? value.Value.GetDisplayName() : null,
                        value => CrewRankExtensions.ParseDisplayName(value))
                    .HasMaxLength(50);
                entity.Property(x => x.PrimaryQualification)
                    .HasConversion<string>()
                    .HasMaxLength(80);
                entity.HasOne<ApplicationUser>()
                    .WithMany()
                    .HasForeignKey(x => x.ApplicationUserId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.EmployerOperator)
                    .WithMany()
                    .HasForeignKey(x => x.EmployerOperatorId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasQueryFilter(x => !x.IsDeleted);
            });

            builder.Entity<CrewDocument>(entity =>
            {
                entity.ToTable("CrewDocuments");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.CrewMemberId, x.DocumentType });
                entity.HasIndex(x => x.ExpiryDate);
                entity.Property(x => x.DocumentType).HasConversion<string>().HasMaxLength(50);
                entity.HasOne(x => x.CrewMember)
                    .WithMany(x => x.Documents)
                    .HasForeignKey(x => x.CrewMemberId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(x => !x.IsDeleted);
            });

            builder.Entity<CrewAssignment>(entity =>
            {
                entity.ToTable("CrewAssignments");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.VesselId, x.Status });
                entity.HasIndex(x => new { x.CrewMemberId, x.Status });
                entity.HasIndex(x => x.SignOnDateUtc);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.RankOnAssignment)
                    .HasConversion(value => value.GetDisplayName(),
                                   value => CrewRankExtensions.ParseDisplayName(value) ?? CrewRank.OrdinarySeaman)
                    .HasMaxLength(50);
                entity.HasOne(x => x.CrewMember)
                    .WithMany(x => x.Assignments)
                    .HasForeignKey(x => x.CrewMemberId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.Vessel)
                    .WithMany(x => x.CrewAssignments)
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.ApprovingComplianceResult)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovingComplianceResultId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasQueryFilter(x => !x.IsDeleted);
            });

            builder.Entity<VesselComplianceSnapshot>(entity =>
            {
                entity.ToTable("VesselComplianceSnapshots");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.VesselId, x.EvaluatedOnUtc });
                entity.Property(x => x.State).HasConversion<string>().HasMaxLength(30);
                entity.HasOne(x => x.Vessel)
                    .WithMany(x => x.ComplianceSnapshots)
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasQueryFilter(x => !x.Vessel!.IsDeleted);
            });

            builder.Entity<ComplianceRule>(entity =>
            {
                entity.ToTable("ComplianceRules");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.Scope, x.IsActive });
                entity.Property(x => x.Scope).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.RequiredDocumentType).HasConversion<string>().HasMaxLength(50);
                entity.Property(x => x.RequiredForRank)
                    .HasConversion(
                        value => value.HasValue ? value.Value.GetDisplayName() : null,
                        value => CrewRankExtensions.ParseDisplayName(value))
                    .HasMaxLength(50);
                entity.HasOne(x => x.RequiredForOperator)
                    .WithMany()
                    .HasForeignKey(x => x.RequiredForOperatorId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.RequiredCourse)
                    .WithMany()
                    .HasForeignKey(x => x.RequiredCourseId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<ComplianceResult>(entity =>
            {
                entity.ToTable("ComplianceResults");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.CrewMemberId, x.EvaluatedOnUtc });
                entity.HasIndex(x => x.VesselId);
                entity.Property(x => x.State).HasConversion<string>().HasMaxLength(30);
                entity.HasOne(x => x.CrewMember)
                    .WithMany(x => x.ComplianceResults)
                    .HasForeignKey(x => x.CrewMemberId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(x => x.Vessel)
                    .WithMany()
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.GeneratedInvoice)
                    .WithMany()
                    .HasForeignKey(x => x.GeneratedInvoiceId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasQueryFilter(x => !x.CrewMember!.IsDeleted);
            });

            builder.Entity<CertificateExpiryEvent>(entity =>
            {
                entity.ToTable("CertificateExpiryEvents");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.Status, x.ExpiryDateUtc });
                entity.HasIndex(x => x.CrewMemberId);
                entity.Property(x => x.Source).HasConversion<string>().HasMaxLength(40);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.HasOne(x => x.CrewMember)
                    .WithMany()
                    .HasForeignKey(x => x.CrewMemberId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(x => x.TrainingCertificate)
                    .WithMany()
                    .HasForeignKey(x => x.TrainingCertificateId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.CrewDocument)
                    .WithMany()
                    .HasForeignKey(x => x.CrewDocumentId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasQueryFilter(x => !x.CrewMember!.IsDeleted);
            });

            builder.Entity<ComplianceAuditEntry>(entity =>
            {
                entity.ToTable("ComplianceAuditEntries");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.Action, x.OccurredOnUtc });
                entity.HasIndex(x => x.CrewMemberId);
                entity.HasIndex(x => x.VesselId);
                entity.Property(x => x.Action).HasConversion<string>().HasMaxLength(50);
                entity.HasOne(x => x.CrewMember)
                    .WithMany()
                    .HasForeignKey(x => x.CrewMemberId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.Vessel)
                    .WithMany()
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.ComplianceResult)
                    .WithMany()
                    .HasForeignKey(x => x.ComplianceResultId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }

        private static void ConfigureBillingEntities(ModelBuilder builder)
        {
            builder.Entity<OperatorBillingAccount>(entity =>
            {
                entity.ToTable("OperatorBillingAccounts");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.BunkerOperatorId).IsUnique();
                entity.HasOne(x => x.BunkerOperator)
                    .WithMany()
                    .HasForeignKey(x => x.BunkerOperatorId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Invoice>(entity =>
            {
                entity.ToTable("Invoices");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => x.InvoiceNumber).IsUnique();
                entity.HasIndex(x => new { x.BunkerOperatorId, x.Status });
                entity.Property(x => x.InvoiceNumber).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(30);
                entity.Property(x => x.SubTotal).HasPrecision(18, 2);
                entity.Property(x => x.TaxAmount).HasPrecision(18, 2);
                entity.Property(x => x.TotalAmount).HasPrecision(18, 2);
                entity.HasOne(x => x.OperatorBillingAccount)
                    .WithMany()
                    .HasForeignKey(x => x.OperatorBillingAccountId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.BunkerOperator)
                    .WithMany()
                    .HasForeignKey(x => x.BunkerOperatorId)
                    .OnDelete(DeleteBehavior.Restrict);
                entity.HasOne(x => x.CrewMember)
                    .WithMany()
                    .HasForeignKey(x => x.CrewMemberId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.Vessel)
                    .WithMany()
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasQueryFilter(x => !x.IsDeleted);
            });

            builder.Entity<InvoiceLineItem>(entity =>
            {
                entity.ToTable("InvoiceLineItems");
                entity.HasKey(x => x.Id);
                entity.Property(x => x.Quantity).HasPrecision(18, 2);
                entity.Property(x => x.UnitPrice).HasPrecision(18, 2);
                entity.Property(x => x.LineTotal).HasPrecision(18, 2);
                entity.HasOne(x => x.Invoice)
                    .WithMany(x => x.LineItems)
                    .HasForeignKey(x => x.InvoiceId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.RelatedCourse)
                    .WithMany()
                    .HasForeignKey(x => x.RelatedCourseId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasQueryFilter(x => !x.Invoice!.IsDeleted);
            });

            builder.Entity<NotificationMessage>(entity =>
            {
                entity.ToTable("NotificationMessages");
                entity.HasKey(x => x.Id);
                entity.HasIndex(x => new { x.RecipientUserId, x.Status });
                entity.HasIndex(x => x.CreatedOnUtc);
                entity.Property(x => x.Channel).HasConversion<string>().HasMaxLength(20);
                entity.Property(x => x.Category).HasConversion<string>().HasMaxLength(40);
                entity.Property(x => x.Status).HasConversion<string>().HasMaxLength(20);
            });
        }
    }
}

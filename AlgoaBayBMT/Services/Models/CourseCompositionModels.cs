using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Models
{
    /// <summary>A shared module as it appears in the Course Builder's Module Library panel.</summary>
    public sealed class ModuleLibraryItemModel
    {
        public Guid ModuleId { get; set; }
        public Guid? CurrentVersionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public int VersionNumber { get; set; }
        public ModuleVersionStatus Status { get; set; }
        public bool IsActive { get; set; }
        public bool IsArchived { get; set; }
        public int LessonCount { get; set; }
        public int? EstimatedMinutes { get; set; }

        /// <summary>Distinct courses that reference any version of this module.</summary>
        public int UsageCourseCount { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }

    /// <summary>Where a module is used. Backs the usage indicator and delete protection.</summary>
    public sealed class ModuleUsageModel
    {
        public Guid ModuleId { get; set; }
        public List<ModuleUsageCourseModel> Courses { get; set; } = new();
        public bool HasLearnerEvidence { get; set; }
        public bool CanDelete => Courses.Count == 0 && !HasLearnerEvidence;
    }

    public sealed class ModuleUsageCourseModel
    {
        public Guid CourseId { get; set; }
        public Guid CourseVersionId { get; set; }
        public string CourseCode { get; set; } = string.Empty;
        public string CourseTitle { get; set; } = string.Empty;
        public int CourseVersionNumber { get; set; }
        public CourseVersionStatus CourseVersionStatus { get; set; }
        public int ModuleVersionNumber { get; set; }
    }

    /// <summary>A rank profile (training level) and the crew ranks assigned to it.</summary>
    public sealed class RankProfileEditModel
    {
        public Guid? RankProfileId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int OrderIndex { get; set; }
        public bool IsActive { get; set; } = true;
        public bool IsSystem { get; set; }
        public List<CrewRank> Ranks { get; set; } = new();

        /// <summary>Course versions currently targeting this profile. Blocks deletion when non-zero.</summary>
        public int CourseUsageCount { get; set; }
    }

    /// <summary>Flags ranks that no profile covers, and ranks claimed by more than one.</summary>
    public sealed class RankCoverageModel
    {
        public List<CrewRank> UnassignedRanks { get; set; } = new();
        public List<RankOverlapModel> OverlappingRanks { get; set; } = new();
        public bool IsFullyCovered => UnassignedRanks.Count == 0 && OverlappingRanks.Count == 0;
    }

    public sealed class RankOverlapModel
    {
        public CrewRank CrewRank { get; set; }
        public List<string> ProfileNames { get; set; } = new();
    }

    /// <summary>One module reference inside a course version, as shown in step 2.</summary>
    public sealed class CourseModuleItemModel
    {
        public Guid CourseModuleId { get; set; }
        public Guid ModuleVersionId { get; set; }
        public Guid ModuleId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Category { get; set; }
        public int OrderIndex { get; set; }
        public bool IsRequired { get; set; }
        public int VersionNumber { get; set; }
        public ModuleVersionStatus Status { get; set; }
        public int LessonCount { get; set; }
        public int? EstimatedMinutes { get; set; }

        /// <summary>Exact version reference shown in Review &amp; Publish, e.g. "MOD-0001 v2".</summary>
        public string VersionReference => $"{Code} v{VersionNumber}";
    }

    /// <summary>A rank profile selected by a course, plus its per-module inclusion.</summary>
    public sealed class CourseRankProfileItemModel
    {
        public Guid? CourseRankProfileId { get; set; }
        public Guid RankProfileId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public int OrderIndex { get; set; }
        public bool IsSelected { get; set; }
        public int? TotalDurationMinutes { get; set; }
        public List<CrewRank> Ranks { get; set; } = new();

        /// <summary>Keyed by CourseModuleId. A missing entry means included.</summary>
        public Dictionary<Guid, bool> ModuleInclusion { get; set; } = new();

        public int IncludedModuleCount { get; set; }
    }

    /// <summary>Everything the four Course Builder steps need in one load.</summary>
    public sealed class CourseCompositionModel
    {
        public Guid CourseId { get; set; }
        public Guid CourseVersionId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public string? VersionLabel { get; set; }
        public CourseVersionStatus Status { get; set; }
        public byte[]? RowVersion { get; set; }

        /// <summary>Published course versions are frozen; edits require a new version.</summary>
        public bool IsEditable => Status == CourseVersionStatus.Draft;

        public List<CourseModuleItemModel> Modules { get; set; } = new();
        public List<CourseRankProfileItemModel> RankProfiles { get; set; } = new();
    }

    public enum PublishValidationSeverity
    {
        Warning = 0,
        Error = 1
    }

    /// <summary>Which Course Builder step a validation issue sends the author back to.</summary>
    public enum CourseBuilderStep
    {
        CourseSetup = 1,
        AddModules = 2,
        RankConfiguration = 3,
        ReviewPublish = 4
    }

    public sealed class PublishValidationIssue
    {
        public PublishValidationSeverity Severity { get; set; } = PublishValidationSeverity.Error;
        public CourseBuilderStep Step { get; set; }
        public string Message { get; set; } = string.Empty;
        public string? RankProfileName { get; set; }
    }

    public sealed class PublishValidationResultModel
    {
        public List<PublishValidationIssue> Issues { get; set; } = new();

        /// <summary>Total duration per rank profile, keyed by RankProfileId.</summary>
        public Dictionary<Guid, int> DurationByRankProfile { get; set; } = new();

        public bool CanPublish => !Issues.Any(x => x.Severity == PublishValidationSeverity.Error);
    }

    /// <summary>The deterministic sequence a learner receives for one course version and rank.</summary>
    public sealed class ResolvedTrainingSequenceModel
    {
        public Guid CourseId { get; set; }
        public Guid CourseVersionId { get; set; }

        /// <summary>Null when the learner's rank matched no profile, or they have full access.</summary>
        public Guid? RankProfileId { get; set; }
        public string? RankProfileName { get; set; }

        /// <summary>True when no rank filter was applied and the full course sequence was returned.</summary>
        public bool IsUnfiltered { get; set; }

        public List<ResolvedModuleModel> Modules { get; set; } = new();
        public int TotalDurationMinutes { get; set; }

        public IEnumerable<Guid> AllLessonIds => Modules.SelectMany(x => x.LessonIds);
    }

    public sealed class ResolvedModuleModel
    {
        public Guid CourseModuleId { get; set; }
        public Guid ModuleVersionId { get; set; }
        public Guid ModuleId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public int VersionNumber { get; set; }
        public int OrderIndex { get; set; }
        public bool IsRequired { get; set; }
        public int? EstimatedMinutes { get; set; }
        public List<Guid> LessonIds { get; set; } = new();

        public string VersionReference => $"{Code} v{VersionNumber}";
    }
}

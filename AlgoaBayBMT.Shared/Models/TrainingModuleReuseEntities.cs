namespace AlgoaBayBMT.Shared.Models
{
    /// <summary>
    /// Stable, reusable identity for an authored training module. The module itself carries no
    /// content — every version of the content lives on <see cref="TrainingModuleVersion"/>.
    /// Courses reference module versions through <see cref="CourseModule"/>, which is what allows
    /// one authored module to serve many courses without duplicating lessons or content blocks.
    /// </summary>
    public class TrainingModule
    {
        public Guid ModuleId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Category { get; set; }
        public bool IsActive { get; set; } = true;

        /// <summary>The version authors and course builders work against by default.</summary>
        public Guid? CurrentVersionId { get; set; }

        public string? CreatedByUserId { get; set; }
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string? UpdatedByUserId { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        /// <summary>
        /// Modules are archived, never deleted, once any course references them or any learner
        /// evidence exists against their lessons.
        /// </summary>
        public bool IsArchived { get; set; }
        public string? ArchivedByUserId { get; set; }
        public DateTime? ArchivedOnUtc { get; set; }

        public byte[]? RowVersion { get; set; }

        public TrainingModuleVersion? CurrentVersion { get; set; }
        public ICollection<TrainingModuleVersion> Versions { get; set; } = new List<TrainingModuleVersion>();
    }

    /// <summary>
    /// An ordered reference from a course version to a shared module version. Adding a module to a
    /// course creates one of these rows and nothing else — no lesson or content block is copied.
    /// </summary>
    public class CourseModule
    {
        public Guid CourseModuleId { get; set; }
        public Guid CourseVersionId { get; set; }
        public Guid ModuleVersionId { get; set; }

        /// <summary>Position within the course sequence. The same order applies to every rank profile.</summary>
        public int OrderIndex { get; set; }

        public bool IsRequired { get; set; } = true;

        public CourseVersion? CourseVersion { get; set; }
        public TrainingModuleVersion? ModuleVersion { get; set; }
        public ICollection<CourseRankModule> RankModules { get; set; } = new List<CourseRankModule>();
    }

    /// <summary>
    /// A configurable grouping of crew ranks — a training "level" such as General Crew, Officer,
    /// Senior Officer or Management. Profiles are global and admin-editable; courses opt into the
    /// ones they serve through <see cref="CourseRankProfile"/>.
    /// </summary>
    public class RankProfile
    {
        public Guid RankProfileId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        /// <summary>Level order, ascending from junior to senior. Also breaks ties during resolution.</summary>
        public int OrderIndex { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Seeded by the system. Still fully editable — this only marks its origin.</summary>
        public bool IsSystem { get; set; }

        public string? CreatedByUserId { get; set; }
        public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
        public string? UpdatedByUserId { get; set; }
        public DateTime? UpdatedOnUtc { get; set; }

        public ICollection<RankProfileRank> Ranks { get; set; } = new List<RankProfileRank>();
        public ICollection<CourseRankProfile> CourseRankProfiles { get; set; } = new List<CourseRankProfile>();
    }

    /// <summary>Membership of a single <see cref="CrewRank"/> in a rank profile.</summary>
    public class RankProfileRank
    {
        public Guid RankProfileRankId { get; set; }
        public Guid RankProfileId { get; set; }

        /// <summary>Persisted as the rank's display name, matching every other CrewRank column.</summary>
        public CrewRank CrewRank { get; set; }

        public RankProfile? RankProfile { get; set; }
    }

    /// <summary>A rank profile that a given course version serves.</summary>
    public class CourseRankProfile
    {
        public Guid CourseRankProfileId { get; set; }
        public Guid CourseVersionId { get; set; }
        public Guid RankProfileId { get; set; }

        /// <summary>
        /// Resolution priority. If a learner's rank belongs to more than one profile on this
        /// course, the lowest OrderIndex wins, which keeps resolution deterministic.
        /// </summary>
        public int OrderIndex { get; set; }

        public bool IsActive { get; set; } = true;

        /// <summary>Computed at publish. Duration is per rank, so it cannot live on CourseVersion.</summary>
        public int? TotalDurationMinutes { get; set; }

        public CourseVersion? CourseVersion { get; set; }
        public RankProfile? RankProfile { get; set; }
        public ICollection<CourseRankModule> RankModules { get; set; } = new List<CourseRankModule>();
    }

    /// <summary>
    /// Per-rank inclusion of a course module. A missing row means included — only exclusions and
    /// explicit ticks are stored. There is no per-rank sequence override; ordering always comes
    /// from <see cref="CourseModule.OrderIndex"/>.
    /// </summary>
    public class CourseRankModule
    {
        public Guid CourseRankModuleId { get; set; }
        public Guid CourseRankProfileId { get; set; }
        public Guid CourseModuleId { get; set; }
        public bool IsIncluded { get; set; } = true;
        public byte[]? RowVersion { get; set; }

        public CourseRankProfile? CourseRankProfile { get; set; }
        public CourseModule? CourseModule { get; set; }
    }
}

using AlgoaBayBMT.Data;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Tests.Infrastructure;

/// <summary>Minimal, direct-to-DbContext seed helpers. Deliberately bypass the services under
/// test — these build starting fixtures, not part of what's being asserted.</summary>
internal static class Seed
{
    public static async Task<(TrainingModule Module, TrainingModuleVersion Version)> CreateModuleWithLessonAsync(
        ApplicationDbContext dbContext,
        string code,
        string title = "Module",
        bool publish = true,
        int estimatedMinutes = 30)
    {
        var module = new TrainingModule
        {
            ModuleId = Guid.NewGuid(),
            Code = code,
            Title = title,
            IsActive = true
        };
        dbContext.TrainingModules.Add(module);
        await dbContext.SaveChangesAsync();

        var version = new TrainingModuleVersion
        {
            ModuleVersionId = Guid.NewGuid(),
            ModuleId = module.ModuleId,
            VersionNumber = 1,
            Title = title,
            Status = publish ? ModuleVersionStatus.Published : ModuleVersionStatus.Draft,
            IsActive = true,
            EstimatedMinutes = estimatedMinutes,
            PublishedOnUtc = publish ? DateTime.UtcNow : null
        };
        dbContext.ModuleVersions.Add(version);

        var lesson = new TrainingLesson
        {
            LessonId = Guid.NewGuid(),
            ModuleVersionId = version.ModuleVersionId,
            Title = $"{title} - Lesson 1",
            OrderIndex = 1,
            IsRequired = true,
            IsActive = true
        };
        dbContext.Lessons.Add(lesson);

        var block = new LessonBlock
        {
            LessonBlockId = Guid.NewGuid(),
            LessonId = lesson.LessonId,
            BlockType = LessonBlockType.TextNarrative,
            Title = "Card",
            OrderIndex = 1,
            IsRequired = true,
            IsActive = true
        };
        dbContext.LessonBlocks.Add(block);

        module.CurrentVersionId = version.ModuleVersionId;

        await dbContext.SaveChangesAsync();
        return (module, version);
    }

    public static async Task<(Course Course, CourseVersion Version)> CreateCourseAsync(
        ApplicationDbContext dbContext,
        string code,
        string title = "Course",
        CourseVersionStatus status = CourseVersionStatus.Draft)
    {
        var course = new Course
        {
            CourseId = Guid.NewGuid(),
            Code = code,
            Title = title,
            IsActive = true
        };
        dbContext.Courses.Add(course);
        await dbContext.SaveChangesAsync();

        var version = new CourseVersion
        {
            CourseVersionId = Guid.NewGuid(),
            CourseId = course.CourseId,
            VersionNumber = 1,
            Status = status
        };
        dbContext.CourseVersions.Add(version);

        course.CurrentVersionId = version.CourseVersionId;
        await dbContext.SaveChangesAsync();
        return (course, version);
    }

    public static async Task<RankProfile> CreateRankProfileAsync(
        ApplicationDbContext dbContext,
        string code,
        int orderIndex,
        params CrewRank[] ranks)
    {
        var profile = new RankProfile
        {
            RankProfileId = Guid.NewGuid(),
            Code = code,
            Name = code,
            OrderIndex = orderIndex,
            IsActive = true,
            IsSystem = true
        };
        dbContext.RankProfiles.Add(profile);
        foreach (var rank in ranks)
        {
            dbContext.RankProfileRanks.Add(new RankProfileRank
            {
                RankProfileRankId = Guid.NewGuid(),
                RankProfileId = profile.RankProfileId,
                CrewRank = rank
            });
        }
        await dbContext.SaveChangesAsync();
        return profile;
    }

    public static async Task<CourseModule> AddModuleReferenceAsync(
        ApplicationDbContext dbContext,
        Guid courseVersionId,
        Guid moduleVersionId,
        int orderIndex)
    {
        var courseModule = new CourseModule
        {
            CourseModuleId = Guid.NewGuid(),
            CourseVersionId = courseVersionId,
            ModuleVersionId = moduleVersionId,
            OrderIndex = orderIndex,
            IsRequired = true
        };
        dbContext.CourseModules.Add(courseModule);
        await dbContext.SaveChangesAsync();
        return courseModule;
    }
}

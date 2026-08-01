using AlgoaBayBMT.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Tests;

/// <summary>A module referenced by two courses is one TrainingModule/TrainingModuleVersion row
/// and two CourseModule reference rows — no lesson or block is ever duplicated.</summary>
public sealed class ReuseTests
{
    [Fact]
    public async Task AddingSameModuleToTwoCourses_CreatesOneVersionAndTwoReferences()
    {
        using var fixture = new SqliteTestFixture();
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();

        var (_, moduleVersion) = await Seed.CreateModuleWithLessonAsync(dbContext, "MOD-0001");
        var (_, courseA) = await Seed.CreateCourseAsync(dbContext, "CRS-A");
        var (_, courseB) = await Seed.CreateCourseAsync(dbContext, "CRS-B");

        var composition = fixture.CreateCompositionService();
        var resultA = await composition.AddModuleToCourseAsync(courseA.CourseVersionId, moduleVersion.ModuleVersionId, "tester");
        var resultB = await composition.AddModuleToCourseAsync(courseB.CourseVersionId, moduleVersion.ModuleVersionId, "tester");

        Assert.True(resultA.Succeeded, resultA.Message);
        Assert.True(resultB.Succeeded, resultB.Message);

        await using var verify = await fixture.DbContextFactory.CreateDbContextAsync();
        Assert.Equal(1, await verify.ModuleVersions.CountAsync(x => x.ModuleId == moduleVersion.ModuleId));
        Assert.Equal(2, await verify.CourseModules.CountAsync(x => x.ModuleVersionId == moduleVersion.ModuleVersionId));
        Assert.Equal(1, await verify.Lessons.CountAsync(x => x.ModuleVersionId == moduleVersion.ModuleVersionId));
        Assert.Equal(1, await verify.LessonBlocks.CountAsync());
    }
}

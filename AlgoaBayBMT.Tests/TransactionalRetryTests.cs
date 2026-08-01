using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Tests;

/// <summary>
/// Requires LocalDB. Drives the composition/publish path through a DbContextFactory configured
/// with EnableRetryOnFailure(), matching Program.cs exactly — this is the one thing the SQLite
/// fixture used everywhere else in this suite structurally cannot do, since SQLite has no retry
/// execution strategy. Every method under test here goes through
/// TransactionalExecution.ExecuteAsync (CourseCompositionService.cs), which previously crashed
/// with "SqlServerRetryingExecutionStrategy does not support user-initiated transactions" the
/// moment EnableRetryOnFailure() was present — a bug invisible to any test or smoke-check that
/// doesn't configure retry the same way production does.
/// </summary>
public sealed class TransactionalRetryTests
{
    [Fact]
    public async Task AddModuleAndPublish_SucceedUnderTheProductionRetryStrategy()
    {
        using var fixture = new LocalDbTestFixture();
        Guid courseVersionId;
        Guid courseId;
        Guid moduleVersionId;
        Guid rankProfileId;

        await using (var dbContext = await fixture.DbContextFactory.CreateDbContextAsync())
        {
            var (module, version) = await Seed.CreateModuleWithLessonAsync(dbContext, "MOD-RETRY");
            moduleVersionId = version.ModuleVersionId;

            var (course, courseVersion) = await Seed.CreateCourseAsync(dbContext, "CRS-RETRY");
            course.Description = "Exercises the retry-configured transactional path.";
            courseId = course.CourseId;
            courseVersionId = courseVersion.CourseVersionId;
            await dbContext.SaveChangesAsync();

            var profile = await Seed.CreateRankProfileAsync(dbContext, "GENERAL_CREW", 1, CrewRank.OrdinarySeaman);
            rankProfileId = profile.RankProfileId;
        }

        var composition = fixture.CreateCompositionServiceWithRetry();

        var addResult = await composition.AddModuleToCourseAsync(courseVersionId, moduleVersionId, "tester");
        Assert.True(addResult.Succeeded, addResult.Message);

        var rankResult = await composition.SetCourseRankProfilesAsync(courseVersionId, [rankProfileId], "tester");
        Assert.True(rankResult.Succeeded, rankResult.Message);

        var validation = await composition.ValidateForPublishAsync(courseVersionId);
        Assert.True(validation.CanPublish, string.Join("; ", validation.Issues.Select(x => x.Message)));

        var publishResult = await composition.PublishAsync(courseId, courseVersionId, "tester");
        Assert.True(publishResult.Succeeded, publishResult.Message);

        await using var verify = await fixture.DbContextFactory.CreateDbContextAsync();
        var status = await verify.CourseVersions.AsNoTracking()
            .Where(x => x.CourseVersionId == courseVersionId)
            .Select(x => x.Status)
            .FirstAsync();
        Assert.Equal(CourseVersionStatus.Published, status);
    }
}

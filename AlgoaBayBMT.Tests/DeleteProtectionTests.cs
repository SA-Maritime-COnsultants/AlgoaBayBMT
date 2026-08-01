using AlgoaBayBMT.Data;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Tests;

/// <summary>A shared module can't be hard-deleted while any course references it or any learner
/// has evidence against its lessons — archiving is the only path once either is true.</summary>
public sealed class DeleteProtectionTests
{
    [Fact]
    public async Task UnusedModule_DeletesCleanly()
    {
        using var fixture = new SqliteTestFixture();
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();
        var (module, _) = await Seed.CreateModuleWithLessonAsync(dbContext, "MOD-UNUSED", publish: false);

        var library = fixture.CreateModuleLibraryService();
        var result = await library.DeleteModuleAsync(module.ModuleId, "tester");

        Assert.True(result.Succeeded, result.Message);
    }

    [Fact]
    public async Task ModuleReferencedByCourse_CannotBeDeleted_ArchiveSucceedsInstead()
    {
        using var fixture = new SqliteTestFixture();
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();
        var (module, version) = await Seed.CreateModuleWithLessonAsync(dbContext, "MOD-USED");
        var (_, courseVersion) = await Seed.CreateCourseAsync(dbContext, "CRS-USES-IT");
        await Seed.AddModuleReferenceAsync(dbContext, courseVersion.CourseVersionId, version.ModuleVersionId, 1);

        var library = fixture.CreateModuleLibraryService();
        var deleteResult = await library.DeleteModuleAsync(module.ModuleId, "tester");
        Assert.False(deleteResult.Succeeded);

        var archiveResult = await library.ArchiveModuleAsync(module.ModuleId, true, "tester");
        Assert.True(archiveResult.Succeeded, archiveResult.Message);
    }

    [Fact]
    public async Task ModuleWithLearnerEvidence_CannotBeDeleted()
    {
        using var fixture = new SqliteTestFixture();
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();
        var (module, version) = await Seed.CreateModuleWithLessonAsync(dbContext, "MOD-EVIDENCE", publish: false);
        var lesson = await dbContext.Lessons.FirstAsync(x => x.ModuleVersionId == version.ModuleVersionId);

        var learner = new ApplicationUser { Id = "some-learner", UserName = "learner@test.local", Email = "learner@test.local" };
        dbContext.Users.Add(learner);

        dbContext.UserLessonProgress.Add(new UserLessonProgress
        {
            UserLessonProgressId = Guid.NewGuid(),
            UserId = learner.Id,
            LessonId = lesson.LessonId,
            Status = ProgressStatus.Completed,
            PercentComplete = 100m
        });
        await dbContext.SaveChangesAsync();

        var library = fixture.CreateModuleLibraryService();
        var deleteResult = await library.DeleteModuleAsync(module.ModuleId, "tester");

        Assert.False(deleteResult.Succeeded);
    }
}

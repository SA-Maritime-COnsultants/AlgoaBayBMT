using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Tests;

/// <summary>A published module version's lessons and content blocks cannot be edited, deleted or
/// reordered — the guarantee the whole shared-module design depends on (F4/F5 in the design doc:
/// UserLessonProgress.CompletionEvidenceJson references these GUIDs directly and is never
/// migrated).</summary>
public sealed class ImmutabilityTests
{
    [Fact]
    public async Task PublishedModuleVersion_RejectsLessonAndBlockMutations()
    {
        using var fixture = new SqliteTestFixture();
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();
        var (_, version) = await Seed.CreateModuleWithLessonAsync(dbContext, "MOD-0001", publish: true);
        var lesson = await dbContext.Lessons.AsNoTracking().FirstAsync(x => x.ModuleVersionId == version.ModuleVersionId);
        var block = await dbContext.LessonBlocks.AsNoTracking().FirstAsync(x => x.LessonId == lesson.LessonId);

        var management = fixture.CreateTrainingManagementService();

        var saveLesson = await management.SaveLessonAsync(new TrainingLessonEditModel
        {
            LessonId = lesson.LessonId,
            ModuleVersionId = version.ModuleVersionId,
            Title = "Edited title"
        }, "tester");
        Assert.False(saveLesson.Succeeded);

        var deleteLesson = await management.DeleteLessonAsync(lesson.LessonId, "tester");
        Assert.False(deleteLesson.Succeeded);

        var moveLesson = await management.MoveLessonAsync(lesson.LessonId, 1, "tester");
        Assert.False(moveLesson.Succeeded);

        var saveBlock = await management.SaveLessonBlockAsync(new TrainingLessonBlockEditModel
        {
            LessonBlockId = block.LessonBlockId,
            LessonId = lesson.LessonId,
            Title = "Edited block"
        }, "tester");
        Assert.False(saveBlock.Succeeded);

        var deleteBlock = await management.DeleteLessonBlockAsync(block.LessonBlockId, "tester");
        Assert.False(deleteBlock.Succeeded);

        var moveBlock = await management.MoveLessonBlockAsync(block.LessonBlockId, 1, "tester");
        Assert.False(moveBlock.Succeeded);

        // Nothing was actually changed in the database.
        await using var verify = await fixture.DbContextFactory.CreateDbContextAsync();
        var unchangedLesson = await verify.Lessons.AsNoTracking().FirstAsync(x => x.LessonId == lesson.LessonId);
        Assert.Equal(lesson.Title, unchangedLesson.Title);
        Assert.Equal(1, await verify.Lessons.CountAsync(x => x.ModuleVersionId == version.ModuleVersionId));
        Assert.Equal(1, await verify.LessonBlocks.CountAsync(x => x.LessonId == lesson.LessonId));
    }

    [Fact]
    public async Task CreateModuleVersion_ClonesContentWithNewIdsAndLeavesSourceVersionIntact()
    {
        using var fixture = new SqliteTestFixture();
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();
        var (module, v1) = await Seed.CreateModuleWithLessonAsync(dbContext, "MOD-0001", publish: true);
        var originalLesson = await dbContext.Lessons.AsNoTracking().FirstAsync(x => x.ModuleVersionId == v1.ModuleVersionId);
        var originalBlock = await dbContext.LessonBlocks.AsNoTracking().FirstAsync(x => x.LessonId == originalLesson.LessonId);

        var library = fixture.CreateModuleLibraryService();
        var result = await library.CreateModuleVersionAsync(module.ModuleId, "tester");

        Assert.True(result.Succeeded, result.Message);
        var v2 = result.Data!;
        Assert.Equal(2, v2.VersionNumber);
        Assert.Equal(ModuleVersionStatus.Draft, v2.Status);

        await using var verify = await fixture.DbContextFactory.CreateDbContextAsync();
        var v2Lesson = await verify.Lessons.AsNoTracking().FirstAsync(x => x.ModuleVersionId == v2.ModuleVersionId);
        var v2Block = await verify.LessonBlocks.AsNoTracking().FirstAsync(x => x.LessonId == v2Lesson.LessonId);

        Assert.NotEqual(originalLesson.LessonId, v2Lesson.LessonId);
        Assert.NotEqual(originalBlock.LessonBlockId, v2Block.LessonBlockId);
        Assert.Equal(originalLesson.Title, v2Lesson.Title);

        // The source version's lessons/blocks are byte-identical, still there, still published.
        var v1StillThere = await verify.Lessons.AsNoTracking().FirstOrDefaultAsync(x => x.LessonId == originalLesson.LessonId);
        Assert.NotNull(v1StillThere);
        var v1Status = await verify.ModuleVersions.AsNoTracking().Where(x => x.ModuleVersionId == v1.ModuleVersionId).Select(x => x.Status).FirstAsync();
        Assert.Equal(ModuleVersionStatus.Published, v1Status);
    }
}

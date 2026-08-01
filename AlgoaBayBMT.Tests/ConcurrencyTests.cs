using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Tests.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Tests;

/// <summary>
/// Requires a real SQL Server instance — SQLite has no rowversion semantics, so a concurrency
/// test against it would silently pass without ever throwing. Run against LocalDB.
///
/// These tests confirm CourseVersion.RowVersion / TrainingModuleVersion.RowVersion are wired as
/// real EF concurrency tokens (catches someone dropping .IsRowVersion() from
/// ApplicationDbContext) — they do NOT exercise PublishAsync's own
/// catch (DbUpdateConcurrencyException) -> OperationResult.Failure handling
/// (CourseCompositionService.cs:558-566), which requires two true concurrent publish calls
/// racing inside the same TransactionalExecution delegate rather than two sequential contexts.
/// That catch block is verified by code inspection only.
/// </summary>
public sealed class ConcurrencyTests
{
    [Fact]
    public async Task CourseVersionRowVersion_IsConfiguredAsConcurrencyToken()
    {
        using var fixture = new LocalDbTestFixture();
        Guid courseVersionId;

        await using (var setup = await fixture.DbContextFactory.CreateDbContextAsync())
        {
            var (_, version) = await Seed.CreateCourseAsync(setup, "CRS-CONCURRENCY");
            courseVersionId = version.CourseVersionId;
        }

        await using var editorA = await fixture.DbContextFactory.CreateDbContextAsync();
        await using var editorB = await fixture.DbContextFactory.CreateDbContextAsync();

        var versionInA = await editorA.CourseVersions.FirstAsync(x => x.CourseVersionId == courseVersionId);
        var versionInB = await editorB.CourseVersions.FirstAsync(x => x.CourseVersionId == courseVersionId);

        versionInA.ChangeSummary = "Editor A's change";
        await editorA.SaveChangesAsync();

        versionInB.ChangeSummary = "Editor B's change, based on stale data";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => editorB.SaveChangesAsync());
    }

    [Fact]
    public async Task ModuleVersionRowVersion_IsConfiguredAsConcurrencyToken()
    {
        using var fixture = new LocalDbTestFixture();
        Guid moduleVersionId;

        await using (var setup = await fixture.DbContextFactory.CreateDbContextAsync())
        {
            var (_, version) = await Seed.CreateModuleWithLessonAsync(setup, "MOD-CONCURRENCY", publish: false);
            moduleVersionId = version.ModuleVersionId;
        }

        await using var editorA = await fixture.DbContextFactory.CreateDbContextAsync();
        await using var editorB = await fixture.DbContextFactory.CreateDbContextAsync();

        var versionInA = await editorA.ModuleVersions.FirstAsync(x => x.ModuleVersionId == moduleVersionId);
        var versionInB = await editorB.ModuleVersions.FirstAsync(x => x.ModuleVersionId == moduleVersionId);

        versionInA.Status = ModuleVersionStatus.Published;
        versionInA.PublishedOnUtc = DateTime.UtcNow;
        await editorA.SaveChangesAsync();

        versionInB.Title = "Changed based on stale data";
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(() => editorB.SaveChangesAsync());
    }
}

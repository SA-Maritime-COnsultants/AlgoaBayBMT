using AlgoaBayBMT.Data;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;

namespace AlgoaBayBMT.Tests;

/// <summary>Mirrors the worked example from the design spec: General Crew gets modules 1 and 4,
/// Officer gets 1/2/4, Management gets 1/2/3/4 — levels are NOT cumulative, each is ticked
/// explicitly. Same course version + rank must always resolve to the same ordered sequence.</summary>
public sealed class ResolutionDeterminismTests
{
    private const string UserId = "learner-1";

    private static async Task<(Guid CourseId, Guid CourseVersionId, Guid[] ModuleVersionIds, Guid[] RankProfileIds)> BuildCourseAsync(SqliteTestFixture fixture)
    {
        await using var dbContext = await fixture.DbContextFactory.CreateDbContextAsync();

        var (_, course) = await Seed.CreateCourseAsync(dbContext, "CRS-LEVELS");
        var modules = new Guid[4];
        for (var i = 0; i < 4; i++)
        {
            var (_, version) = await Seed.CreateModuleWithLessonAsync(dbContext, $"MOD-000{i + 1}", $"Module {i + 1}");
            modules[i] = version.ModuleVersionId;
        }

        var generalCrew = await Seed.CreateRankProfileAsync(dbContext, "GENERAL_CREW", 1, CrewRank.OrdinarySeaman);
        var officer = await Seed.CreateRankProfileAsync(dbContext, "OFFICER", 2, CrewRank.ChiefOfficer);
        var seniorOfficer = await Seed.CreateRankProfileAsync(dbContext, "SENIOR_OFFICER", 3, CrewRank.Captain);
        var management = await Seed.CreateRankProfileAsync(dbContext, "MANAGEMENT", 4, CrewRank.POAC);

        var composition = fixture.CreateCompositionService();
        foreach (var moduleVersionId in modules)
        {
            await composition.AddModuleToCourseAsync(course.CourseVersionId, moduleVersionId, "tester");
        }

        var courseModules = (await composition.GetCompositionAsync(course.CourseId))!.Modules;
        var courseModuleByOrder = courseModules.OrderBy(x => x.OrderIndex).ToList();

        await composition.SetCourseRankProfilesAsync(course.CourseVersionId,
            [generalCrew.RankProfileId, officer.RankProfileId, seniorOfficer.RankProfileId, management.RankProfileId], "tester");

        var afterSelect = (await composition.GetCompositionAsync(course.CourseId))!;
        Guid CourseRankProfileIdFor(Guid rankProfileId) => afterSelect.RankProfiles.First(x => x.RankProfileId == rankProfileId).CourseRankProfileId!.Value;

        // General Crew: modules 1, 4 only -> exclude 2 and 3.
        var generalCrpId = CourseRankProfileIdFor(generalCrew.RankProfileId);
        await composition.SetRankModuleInclusionAsync(generalCrpId, courseModuleByOrder[1].CourseModuleId, false, "tester");
        await composition.SetRankModuleInclusionAsync(generalCrpId, courseModuleByOrder[2].CourseModuleId, false, "tester");

        // Officer: modules 1, 2, 4 -> exclude 3.
        var officerCrpId = CourseRankProfileIdFor(officer.RankProfileId);
        await composition.SetRankModuleInclusionAsync(officerCrpId, courseModuleByOrder[2].CourseModuleId, false, "tester");

        // Management: all four modules, nothing excluded.

        return (course.CourseId, course.CourseVersionId, modules,
            [generalCrew.RankProfileId, officer.RankProfileId, seniorOfficer.RankProfileId, management.RankProfileId]);
    }

    [Fact]
    public async Task GeneralCrew_ResolvesToModulesOneAndFour()
    {
        using var fixture = new SqliteTestFixture();
        var (_, courseVersionId, modules, _) = await BuildCourseAsync(fixture);

        var resolver = fixture.CreateResolutionService();
        var resolved = await resolver.ResolveAsync(courseVersionId, CrewRank.OrdinarySeaman);

        Assert.NotNull(resolved);
        Assert.False(resolved!.IsUnfiltered);
        Assert.Equal([modules[0], modules[3]], resolved.Modules.Select(x => x.ModuleVersionId).ToArray());
    }

    [Fact]
    public async Task Officer_ResolvesToModulesOneTwoFour()
    {
        using var fixture = new SqliteTestFixture();
        var (_, courseVersionId, modules, _) = await BuildCourseAsync(fixture);

        var resolver = fixture.CreateResolutionService();
        var resolved = await resolver.ResolveAsync(courseVersionId, CrewRank.ChiefOfficer);

        Assert.Equal([modules[0], modules[1], modules[3]], resolved!.Modules.Select(x => x.ModuleVersionId).ToArray());
    }

    [Fact]
    public async Task Management_ResolvesToAllFourModulesInOrder()
    {
        using var fixture = new SqliteTestFixture();
        var (_, courseVersionId, modules, _) = await BuildCourseAsync(fixture);

        var resolver = fixture.CreateResolutionService();
        var resolved = await resolver.ResolveAsync(courseVersionId, CrewRank.POAC);

        Assert.Equal(modules, resolved!.Modules.Select(x => x.ModuleVersionId).ToArray());
    }

    [Fact]
    public async Task NullRank_ResolvesUnfilteredWithNoRankProfile()
    {
        using var fixture = new SqliteTestFixture();
        var (_, courseVersionId, modules, _) = await BuildCourseAsync(fixture);

        var resolver = fixture.CreateResolutionService();
        var resolved = await resolver.ResolveAsync(courseVersionId, crewRank: null);

        Assert.True(resolved!.IsUnfiltered);
        Assert.Null(resolved.RankProfileId);
        Assert.Equal(modules, resolved.Modules.Select(x => x.ModuleVersionId).ToArray());
    }

    [Fact]
    public async Task SameCourseVersionAndRank_AlwaysResolvesToTheIdenticalSequence()
    {
        using var fixture = new SqliteTestFixture();
        var (_, courseVersionId, _, _) = await BuildCourseAsync(fixture);
        var resolver = fixture.CreateResolutionService();

        var first = await resolver.ResolveAsync(courseVersionId, CrewRank.ChiefOfficer);
        var second = await resolver.ResolveAsync(courseVersionId, CrewRank.ChiefOfficer);

        Assert.Equal(
            first!.Modules.Select(x => x.ModuleVersionId),
            second!.Modules.Select(x => x.ModuleVersionId));
    }
}

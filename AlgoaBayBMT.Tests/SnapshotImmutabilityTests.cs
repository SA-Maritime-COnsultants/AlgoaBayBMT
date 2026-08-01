using AlgoaBayBMT.Data;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Tests.Infrastructure;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Tests;

/// <summary>Once a learner's assignment is pinned, republishing the course with a different
/// module set must not change what that learner sees — the whole point of pinning
/// ResolvedCourseVersionId/ResolvedRankProfileId instead of re-deriving live every time.</summary>
public sealed class SnapshotImmutabilityTests
{
    [Fact]
    public async Task PinnedAssignment_UnaffectedByLaterCourseVersionWithDifferentModules()
    {
        using var fixture = new SqliteTestFixture();

        Guid moduleV1;
        Guid courseId;
        Guid firstCourseVersionId;

        await using (var dbContext = await fixture.DbContextFactory.CreateDbContextAsync())
        {
            var user = new ApplicationUser { Id = "learner-2", UserName = "learner2@test.local", Email = "learner2@test.local" };
            dbContext.Users.Add(user);

            var (_, version) = await Seed.CreateModuleWithLessonAsync(dbContext, "MOD-A");
            moduleV1 = version.ModuleVersionId;
            var (course, courseVersion) = await Seed.CreateCourseAsync(dbContext, "CRS-PIN", status: CourseVersionStatus.Published);
            courseId = course.CourseId;
            firstCourseVersionId = courseVersion.CourseVersionId;

            await dbContext.CourseModules.AddAsync(new CourseModule
            {
                CourseModuleId = Guid.NewGuid(),
                CourseVersionId = courseVersion.CourseVersionId,
                ModuleVersionId = moduleV1,
                OrderIndex = 1,
                IsRequired = true
            });

            dbContext.UserTrainingAssignments.Add(new UserTrainingAssignment
            {
                UserTrainingAssignmentId = Guid.NewGuid(),
                UserId = user.Id,
                CourseId = course.CourseId,
                Status = AssignmentStatus.Assigned
            });

            await dbContext.SaveChangesAsync();
        }

        var resolver = fixture.CreateResolutionService();
        var learnerUserId = "learner-2";

        var beforeRepublish = await resolver.ResolveForLearnerAsync(learnerUserId, courseId);
        Assert.NotNull(beforeRepublish);
        Assert.Equal([moduleV1], beforeRepublish!.Modules.Select(x => x.ModuleVersionId).ToArray());

        // Republish the course as a new version with a completely different module set.
        Guid newCourseVersionId;
        await using (var dbContext = await fixture.DbContextFactory.CreateDbContextAsync())
        {
            var (_, differentModuleVersion) = await Seed.CreateModuleWithLessonAsync(dbContext, "MOD-B");
            var newVersion = new CourseVersion
            {
                CourseVersionId = Guid.NewGuid(),
                CourseId = courseId,
                VersionNumber = 2,
                Status = CourseVersionStatus.Published
            };
            dbContext.CourseVersions.Add(newVersion);
            newCourseVersionId = newVersion.CourseVersionId;
            dbContext.CourseModules.Add(new CourseModule
            {
                CourseModuleId = Guid.NewGuid(),
                CourseVersionId = newVersion.CourseVersionId,
                ModuleVersionId = differentModuleVersion.ModuleVersionId,
                OrderIndex = 1,
                IsRequired = true
            });

            var course = await dbContext.Courses.FirstAsync(x => x.CourseId == courseId);
            course.CurrentVersionId = newVersion.CourseVersionId;
            await dbContext.SaveChangesAsync();
        }

        var afterRepublish = await resolver.ResolveForLearnerAsync(learnerUserId, courseId);

        Assert.NotNull(afterRepublish);
        Assert.Equal(firstCourseVersionId, afterRepublish!.CourseVersionId);
        Assert.NotEqual(newCourseVersionId, afterRepublish.CourseVersionId);
        Assert.Equal([moduleV1], afterRepublish.Modules.Select(x => x.ModuleVersionId).ToArray());
    }
}

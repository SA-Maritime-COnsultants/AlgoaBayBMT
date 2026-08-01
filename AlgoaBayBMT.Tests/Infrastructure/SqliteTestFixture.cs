using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace AlgoaBayBMT.Tests.Infrastructure;

/// <summary>
/// An isolated SQLite in-memory database per test, wired up with the same DbContextFactory
/// pattern the services require plus a real Identity UserManager (TrainingResolutionService
/// calls UserManager.IsInRoleAsync/FindByIdAsync — a fake stand-in wouldn't exercise the same
/// code path). Not EF InMemory: SQLite honours unique indexes and real transactions, which is
/// what most of these invariants actually depend on.
/// </summary>
public sealed class SqliteTestFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ServiceProvider _serviceProvider;

    public SqliteTestFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var services = new ServiceCollection();
        services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlite(_connection));
        services.AddLogging();
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        services.AddScoped<IRankProfileService, RankProfileService>();
        services.AddScoped<IModuleLibraryService, ModuleLibraryService>();
        services.AddScoped<ITrainingResolutionService, TrainingResolutionService>();
        services.AddScoped<ICourseCompositionService, CourseCompositionService>();

        _serviceProvider = services.BuildServiceProvider();

        DbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();
        using var dbContext = DbContextFactory.CreateDbContext();
        dbContext.Database.EnsureCreated();
    }

    public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; }

    public UserManager<ApplicationUser> CreateUserManager()
        => _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    public IRankProfileService CreateRankProfileService()
        => new RankProfileService(DbContextFactory, NullLogger<RankProfileService>.Instance);

    public IModuleLibraryService CreateModuleLibraryService()
        => new ModuleLibraryService(DbContextFactory, NullLogger<ModuleLibraryService>.Instance);

    public ITrainingResolutionService CreateResolutionService()
        => new TrainingResolutionService(DbContextFactory, CreateUserManager(), CreateRankProfileService(), NullLogger<TrainingResolutionService>.Instance);

    public ICourseCompositionService CreateCompositionService()
        => new CourseCompositionService(DbContextFactory, CreateResolutionService(), NullLogger<CourseCompositionService>.Instance);

    public RoleManager<IdentityRole> CreateRoleManager()
        => _serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

    public ITrainingManagementService CreateTrainingManagementService()
        => new TrainingManagementService(DbContextFactory, CreateRoleManager(), new NoopAssetStorageService(), NullLogger<TrainingManagementService>.Instance);

    /// <summary>Content mutators under test never touch asset storage; this exists only to
    /// satisfy TrainingManagementService's constructor.</summary>
    private sealed class NoopAssetStorageService : ITrainingAssetStorageService
    {
        public Task<OperationResult<TrainingMediaUploadModel>> SaveCourseThumbnailAsync(IBrowserFile file, string? uploadedByUserId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<OperationResult<TrainingMediaUploadModel>> SaveLessonAssetAsync(IBrowserFile file, string assetCategory, string? uploadedByUserId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<OperationResult<TrainingMediaUploadModel>> SaveLessonAvatarVideoAsync(IBrowserFile file, string? uploadedByUserId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<OperationResult<TrainingMediaUploadModel>> SaveRichTextImageAsync(IBrowserFile file, string? uploadedByUserId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<OperationResult<TrainingMediaUploadModel>> SaveRichTextImageAsync(IFormFile file, string? uploadedByUserId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task<OperationResult<TrainingMediaUploadModel>> SaveFlashCardImageAsync(IBrowserFile file, string? uploadedByUserId, CancellationToken cancellationToken = default) => throw new NotSupportedException();
        public Task DeleteMediaAssetAsync(Guid? mediaAssetId, CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task DeleteFilesAsync(IEnumerable<string> urls, CancellationToken cancellationToken = default) => Task.CompletedTask;
    }

    public void Dispose()
    {
        _serviceProvider.Dispose();
        _connection.Dispose();
    }
}

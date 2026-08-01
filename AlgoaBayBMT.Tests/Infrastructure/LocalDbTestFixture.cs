using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services;
using AlgoaBayBMT.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace AlgoaBayBMT.Tests.Infrastructure;

/// <summary>
/// A throwaway LocalDB database, created fresh (EnsureCreated) per test and dropped on dispose.
/// Needed for anything SQLite can't honour: real SQL Server `rowversion` concurrency-token
/// semantics, and (via <see cref="CreateCompositionServiceWithRetry"/>) the actual
/// EnableRetryOnFailure() execution strategy Program.cs configures — SQLite has no retry
/// strategy at all, so the SQLite fixture cannot catch a service method that opens a manual
/// transaction outside CreateExecutionStrategy() (the exact bug TransactionalExecution exists
/// to prevent, previously caught only by live browser testing).
/// </summary>
public sealed class LocalDbTestFixture : IDisposable
{
    private readonly string _databaseName = $"AlgoaBayBMT_Tests_{Guid.NewGuid():N}";
    private readonly ServiceProvider _serviceProvider;
    private readonly string _connectionString;

    public LocalDbTestFixture()
    {
        _connectionString = $"Server=(localdb)\\MSSQLLocalDB;Database={_databaseName};Trusted_Connection=True;MultipleActiveResultSets=true";

        var services = new ServiceCollection();
        services.AddDbContextFactory<ApplicationDbContext>(options => options.UseSqlServer(_connectionString));
        services.AddLogging();
        services.AddIdentityCore<ApplicationUser>()
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>();

        _serviceProvider = services.BuildServiceProvider();
        DbContextFactory = _serviceProvider.GetRequiredService<IDbContextFactory<ApplicationDbContext>>();

        using var dbContext = DbContextFactory.CreateDbContext();
        dbContext.Database.EnsureCreated();
    }

    public IDbContextFactory<ApplicationDbContext> DbContextFactory { get; }

    public UserManager<ApplicationUser> CreateUserManager()
        => _serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    /// <summary>
    /// A CourseCompositionService built on a DbContextFactory configured exactly like
    /// Program.cs (EnableRetryOnFailure), so TransactionalExecution's retry path is real here,
    /// not bypassed the way the plain SqliteTestFixture unavoidably bypasses it.
    /// </summary>
    public ICourseCompositionService CreateCompositionServiceWithRetry()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure())
            .Options;
        var retryFactory = new SimpleDbContextFactory(options);

        var rankProfileService = new RankProfileService(retryFactory, NullLogger<RankProfileService>.Instance);
        var resolutionService = new TrainingResolutionService(retryFactory, CreateUserManager(), rankProfileService, NullLogger<TrainingResolutionService>.Instance);
        return new CourseCompositionService(retryFactory, resolutionService, NullLogger<CourseCompositionService>.Instance);
    }

    public void Dispose()
    {
        using var dbContext = DbContextFactory.CreateDbContext();
        dbContext.Database.EnsureDeleted();
        _serviceProvider.Dispose();
    }

    private sealed class SimpleDbContextFactory(DbContextOptions<ApplicationDbContext> options) : IDbContextFactory<ApplicationDbContext>
    {
        public ApplicationDbContext CreateDbContext() => new(options);
        public Task<ApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default) => Task.FromResult(CreateDbContext());
    }
}

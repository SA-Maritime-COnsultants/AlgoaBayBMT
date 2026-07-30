using AlgoaBayBMT.Components;
using AlgoaBayBMT.Components.Account;
using AlgoaBayBMT.Data;
using AlgoaBayBMT.Emergency.OilSpill;
using AlgoaBayBMT.Services;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.AspNetCore.StaticFiles;
using Syncfusion.Blazor;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddSyncfusionBlazor();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IdentityRedirectManager>();
builder.Services.AddScoped<AuthenticationStateProvider, IdentityRevalidatingAuthenticationStateProvider>();

builder.Services.AddAuthentication(options =>
    {
        options.DefaultScheme = IdentityConstants.ApplicationScheme;
        options.DefaultSignInScheme = IdentityConstants.ExternalScheme;
    })
    .AddIdentityCookies();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContextFactory<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString, sqlOptions => sqlOptions.EnableRetryOnFailure()));
builder.Services.AddScoped(sp => sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentityCore<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = true;
        options.SignIn.RequireConfirmedEmail = true;
        options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddSignInManager()
    .AddDefaultTokenProviders();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(PolicyNames.AdminOnly, policy => policy.RequireRole(RoleNames.Admin));
    options.AddPolicy(PolicyNames.AdminOrCompanyManager, policy => policy.RequireRole(RoleNames.Admin, RoleNames.CompanyManager));
    options.AddPolicy(PolicyNames.CrewComplianceManagement, policy => policy.RequireRole(RoleNames.Admin, RoleNames.CompanyManager, RoleNames.Captain, RoleNames.Co));
    options.AddPolicy(PolicyNames.TrainingApproval, policy => policy.RequireRole(RoleNames.Admin, RoleNames.CompanyManager));
    options.AddPolicy(PolicyNames.BillingManagement, policy => policy.RequireRole(RoleNames.Admin, RoleNames.CompanyManager));
});

builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<IUserApprovalService, UserApprovalService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IRoleEditorService, RoleEditorService>();
builder.Services.AddScoped<ITrainingManagementService, TrainingManagementService>();
builder.Services.AddScoped<ITrainingAssetStorageService, TrainingAssetStorageService>();
builder.Services.AddScoped<ILearnerTrainingService, LearnerTrainingService>();
builder.Services.AddScoped<IContentAuthoringService, ContentAuthoringService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IRandomisationService, RandomisationService>();
builder.Services.AddScoped<IExamService, ExamService>();
builder.Services.AddScoped<ICompetencyService, CompetencyService>();
builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IPortService, PortService>();
builder.Services.AddScoped<IOperatorService, OperatorService>();
builder.Services.AddScoped<IBargeService, BargeService>();
builder.Services.AddScoped<IBargeDeploymentService, BargeDeploymentService>();

// Crewing module
builder.Services.AddScoped<IVesselService, VesselService>();
builder.Services.AddScoped<ICrewService, CrewService>();
builder.Services.AddScoped<IComplianceService, ComplianceService>();
builder.Services.AddScoped<ICrewAssignmentService, CrewAssignmentService>();
builder.Services.AddScoped<ICrewComplianceWorkflowService, CrewComplianceWorkflowService>();
builder.Services.AddScoped<IBunkeringOperationService, BunkeringOperationService>();
builder.Services.AddScoped<IISGOTTTemplateService, ISGOTTTemplateService>();
builder.Services.AddScoped<IISGOTTChecklistService, ISGOTTChecklistService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IRegulatoryReportingService, RegulatoryReportingService>();

// Emergency Management - Oil Spill Modelling & Response
builder.Services.AddOilSpillModule();

builder.Services.AddHostedService<AlgoaBayBMT.Services.Background.CertificateExpiryWatcher>();
builder.Services.AddHostedService<AlgoaBayBMT.Services.Background.NotificationDispatcher>();
builder.Services.AddHostedService<AlgoaBayBMT.Services.Background.ComplianceRevalidator>();

builder.Services.Configure<AlgoaBayBMT.Services.Models.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
builder.Services.AddSingleton<ApplicationEmailService>();
builder.Services.AddSingleton<IApplicationEmailService>(sp => sp.GetRequiredService<ApplicationEmailService>());
builder.Services.AddSingleton<IEmailSender<ApplicationUser>>(sp => sp.GetRequiredService<ApplicationEmailService>());

var app = builder.Build();
//Register Syncfusion license https://help.syncfusion.com/common/essential-studio/licensing/how-to-generate
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JAaF5cX2pCd1p/TH5YfUNzdUVEY1ZUTXxaS1ZhSXxVdkJgWX9YcnNQRmNYVUJ9XEY=");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

var runtimeUploadsRoot = Path.Combine(app.Environment.WebRootPath, "uploads");
Directory.CreateDirectory(runtimeUploadsRoot);
var uploadContentTypeProvider = new FileExtensionContentTypeProvider();
uploadContentTypeProvider.Mappings[".mp4"] = "video/mp4";
uploadContentTypeProvider.Mappings[".webm"] = "video/webm";
uploadContentTypeProvider.Mappings[".mov"] = "video/quicktime";
uploadContentTypeProvider.Mappings[".m4v"] = "video/mp4";
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(runtimeUploadsRoot),
    RequestPath = "/uploads",
    ContentTypeProvider = uploadContentTypeProvider,
    ServeUnknownFileTypes = false,
    OnPrepareResponse = context =>
    {
        context.Context.Response.Headers.Append("Accept-Ranges", "bytes");
        context.Context.Response.Headers.Append("Cache-Control", "public,max-age=3600");
    }
});

app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapGet("/training-media/stream", (string path, IWebHostEnvironment env) =>
{
    var normalizedPath = path.Trim().TrimStart('~').TrimStart('/').Replace("\\", "/");
    if (string.IsNullOrWhiteSpace(normalizedPath) || !normalizedPath.StartsWith("uploads/training", StringComparison.OrdinalIgnoreCase))
    {
        return Results.BadRequest();
    }

    var absolutePath = Path.Combine(env.WebRootPath, normalizedPath.Replace('/', Path.DirectorySeparatorChar));
    if (!File.Exists(absolutePath))
    {
        return Results.NotFound();
    }

    return Results.File(
        absolutePath,
        TrainingAssetStorageService.GetContentTypeForPath(normalizedPath),
        enableRangeProcessing: true);
});

app.MapPost("/training-media/rte-images", async (HttpContext httpContext, ITrainingAssetStorageService trainingAssetStorageService, CancellationToken cancellationToken) =>
{
    if (!httpContext.User.Identity?.IsAuthenticated ?? true)
    {
        return Results.Unauthorized();
    }

    if (!httpContext.Request.HasFormContentType)
    {
        return Results.BadRequest(new { error = new { message = "No form data was supplied." } });
    }

    var form = await httpContext.Request.ReadFormAsync(cancellationToken);
    var file = form.Files.FirstOrDefault();
    if (file is null)
    {
        return Results.BadRequest(new { error = new { message = "No image file was supplied." } });
    }

    var userId = httpContext.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
    var result = await trainingAssetStorageService.SaveRichTextImageAsync(file, userId, cancellationToken);
    if (!result.Succeeded || result.Data is null)
    {
        return Results.BadRequest(new { error = new { message = result.Message ?? string.Join(", ", result.Errors) } });
    }

    var normalizedUrl = result.Data.Url.StartsWith('/') ? result.Data.Url : $"/{result.Data.Url}";

    return Results.Json(new
    {
        file = new
        {
            name = result.Data.FileName,
            url = normalizedUrl,
            size = result.Data.FileSizeBytes,
            saveUrl = normalizedUrl
        },
        path = normalizedUrl,
        url = normalizedUrl
    });
});

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

try
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.MigrateAsync();
    }

    await IdentitySeedData.EnsureSeedDataAsync(app.Services);
}
catch (Exception ex)
{
    app.Logger.LogCritical(ex, "Database migration/seed failed during startup. The web server will still start listening, but data-dependent pages will not work until the database connection is restored.");
}

app.Run();

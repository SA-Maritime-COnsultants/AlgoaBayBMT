using AlgoaBayBMT.Components;
using AlgoaBayBMT.Components.Account;
using AlgoaBayBMT.Data;
using AlgoaBayBMT.Security;
using AlgoaBayBMT.Services;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Security;
using AlgoaBayBMT.Services.Crew;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
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
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));
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
    options.AddPolicy(PolicyNames.AuditAuthorities, policy => policy.RequireRole(RoleNames.Dffe, RoleNames.Tnpa, RoleNames.Samsa));
    options.AddPolicy(PolicyNames.AreaScopedAccess, policy => policy.RequireAuthenticatedUser());
    options.AddPolicy(PolicyNames.VesselCommand, policy => policy.Requirements.Add(new VesselCommandRequirement()));
    options.AddPolicy(PolicyNames.CrewListAccess, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.CompanyManager, RoleNames.Captain, RoleNames.Co));
    options.AddPolicy(PolicyNames.VesselCrewListsAccess, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.CompanyManager));
});

builder.Services.AddScoped<IAuthorizationHandler, VesselCommandHandler>();

builder.Services.AddScoped<IAreaService, AreaService>();
builder.Services.AddScoped<IVesselService, VesselService>();
builder.Services.AddScoped<ICompanyService, CompanyService>();
builder.Services.AddScoped<IUserRegistrationService, UserRegistrationService>();
builder.Services.AddScoped<IUserApprovalService, UserApprovalService>();
builder.Services.AddScoped<IUserManagementService, UserManagementService>();
builder.Services.AddScoped<IVesselRoleAssignmentService, VesselRoleAssignmentService>();
builder.Services.AddScoped<ICrewDirectoryService, CrewDirectoryService>();
builder.Services.AddScoped<ICrewDeploymentService, CrewDeploymentService>();
builder.Services.AddScoped<INotificationRoutingService, NotificationRoutingService>();
builder.Services.AddScoped<IAuthorityContactService, AuthorityContactService>();
builder.Services.AddScoped<IRoleEditorService, RoleEditorService>();
builder.Services.AddScoped<ICrewAssignmentService, CrewAssignmentService>();
builder.Services.AddScoped<ICrewChangeHistoryService, CrewChangeHistoryService>();
builder.Services.AddSingleton<ITrainingComplianceNotifier, NoOpTrainingComplianceNotifier>();
builder.Services.Configure<AlgoaBayBMT.Services.Models.EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
builder.Services.AddSingleton<IEmailTemplateRenderer, EmailTemplateRenderer>();
builder.Services.AddSingleton<ApplicationEmailService>();
builder.Services.AddSingleton<IApplicationEmailService>(sp => sp.GetRequiredService<ApplicationEmailService>());
builder.Services.AddSingleton<IEmailSender<ApplicationUser>>(sp => sp.GetRequiredService<ApplicationEmailService>());

var app = builder.Build();
//Register Syncfusion license https://help.syncfusion.com/common/essential-studio/licensing/how-to-generate
Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("Ngo9BigBOggjHTQxAR8/V1JHaF5cWWRCf1FpRmJGdld5fUVHYVZUTXxaS00DNHVRdkdlWXteeHVQR2BdUUB3XEJWYEo=");

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
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Add additional endpoints required by the Identity /Account Razor components.
app.MapAdditionalIdentityEndpoints();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
}

await IdentitySeedData.EnsureSeedDataAsync(app.Services);

app.Run();

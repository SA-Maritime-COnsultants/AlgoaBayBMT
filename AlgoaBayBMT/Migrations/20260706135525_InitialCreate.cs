using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AuthoringLessons",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthoringLessons", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BunkerAreasOfOperation",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RegionCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EnvironmentalSensitivityRating = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkerAreasOfOperation", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BunkerFuels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkerFuels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BunkerOperators",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CompanyRegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhysicalAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContactPerson = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmergencyContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkerOperators", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ISGOTTStageTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISGOTTStageTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MediaAssets",
                columns: table => new
                {
                    MediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RelativePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FileSizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UploadedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    HashSha256 = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MediaAssets", x => x.MediaAssetId);
                });

            migrationBuilder.CreateTable(
                name: "NotificationMessages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Channel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Category = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RecipientUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    RecipientEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    RecipientDisplayName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Subject = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: false),
                    Body = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RelatedCrewMemberId = table.Column<int>(type: "int", nullable: true),
                    RelatedVesselId = table.Column<int>(type: "int", nullable: true),
                    RelatedComplianceResultId = table.Column<int>(type: "int", nullable: true),
                    RelatedInvoiceId = table.Column<int>(type: "int", nullable: true),
                    RelatedExpiryEventId = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReadOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationMessages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TrainingAuditLogs",
                columns: table => new
                {
                    TrainingAuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ChangedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BeforeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingAuditLogs", x => x.TrainingAuditLogId);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonContentItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LessonId = table.Column<int>(type: "int", nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    FrontText = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BodyHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    BackHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DescriptionHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    VideoUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    QuestionHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ScenarioHtml = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OptionA = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OptionB = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OptionC = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    OptionD = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Option1 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Option2 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Option3 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Option4 = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CorrectOption = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CorrectBool = table.Column<bool>(type: "bit", nullable: true),
                    CorrectOptionIndex = table.Column<int>(type: "int", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonContentItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_LessonContentItems_AuthoringLessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "AuthoringLessons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BunkerPorts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    Depth = table.Column<double>(type: "float", nullable: true),
                    MaxVesselSize = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsAnchorage = table.Column<bool>(type: "bit", nullable: false),
                    IsBunkeringAllowed = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    AreaOfOperationId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkerPorts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BunkerPorts_BunkerAreasOfOperation_AreaOfOperationId",
                        column: x => x.AreaOfOperationId,
                        principalTable: "BunkerAreasOfOperation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    CellNo = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RequestedRole = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    ApprovalStatus = table.Column<int>(type: "int", nullable: false),
                    IsCrew = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Qualification = table.Column<int>(type: "int", nullable: true),
                    CrewRank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SidNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SidIssuingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SidIssuingAuthority = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SidIssueDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SidExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsAccountApproved = table.Column<bool>(type: "bit", nullable: false),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: true),
                    ProfilePicture = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    ProfilePictureContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    RegisteredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    IsCrewManager = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsBunkerManager = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_BunkerOperators_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "BunkerOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BunkerBarges",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IMO = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MMSI = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CallSign = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: true),
                    CapacityMT = table.Column<double>(type: "float", nullable: true),
                    FuelTypesSupported = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    PumpingRate = table.Column<double>(type: "float", nullable: true),
                    LastInspectionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CertificationExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CrewCapacity = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    BunkerOperatorId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkerBarges", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BunkerBarges_BunkerOperators_BunkerOperatorId",
                        column: x => x.BunkerOperatorId,
                        principalTable: "BunkerOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BunkerOperatorAreaAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BunkerOperatorId = table.Column<int>(type: "int", nullable: false),
                    AreaOfOperationId = table.Column<int>(type: "int", nullable: false),
                    AssignedFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignedTo = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkerOperatorAreaAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BunkerOperatorAreaAssignments_BunkerAreasOfOperation_AreaOfOperationId",
                        column: x => x.AreaOfOperationId,
                        principalTable: "BunkerAreasOfOperation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BunkerOperatorAreaAssignments_BunkerOperators_BunkerOperatorId",
                        column: x => x.BunkerOperatorId,
                        principalTable: "BunkerOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OperatorBillingAccounts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BunkerOperatorId = table.Column<int>(type: "int", nullable: false),
                    AccountNumber = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    BillingContactName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    BillingEmail = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    BillingPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BillingAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    PaymentTermsDays = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperatorBillingAccounts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OperatorBillingAccounts_BunkerOperators_BunkerOperatorId",
                        column: x => x.BunkerOperatorId,
                        principalTable: "BunkerOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Vessels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IMO = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    MMSI = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CallSign = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Flag = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    VesselType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    GrossTonnage = table.Column<double>(type: "float", nullable: true),
                    LengthOverall = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: true),
                    LOA = table.Column<double>(type: "float", nullable: true),
                    Beam = table.Column<double>(type: "float(18)", precision: 18, scale: 2, nullable: true),
                    OwningOperatorId = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vessels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vessels_BunkerOperators_OwningOperatorId",
                        column: x => x.OwningOperatorId,
                        principalTable: "BunkerOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ISGOTTItemTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageTemplateId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISGOTTItemTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ISGOTTItemTemplates_ISGOTTStageTemplates_StageTemplateId",
                        column: x => x.StageTemplateId,
                        principalTable: "ISGOTTStageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserPasskeys",
                columns: table => new
                {
                    CredentialId = table.Column<byte[]>(type: "varbinary(1024)", maxLength: 1024, nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserPasskeys", x => x.CredentialId);
                    table.ForeignKey(
                        name: "FK_AspNetUserPasskeys_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    RoleId = table.Column<string>(type: "nvarchar(450)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrewMembers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ApplicationUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    MiddleNames = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    PassportNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportIssuingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PassportExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NationalIdNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    SidNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    SidIssuingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    SidIssuingAuthority = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    SidExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Rank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PrimaryQualification = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    EmployerOperatorId = table.Column<int>(type: "int", nullable: true),
                    EmployerName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(254)", maxLength: 254, nullable: true),
                    Phone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmergencyContactNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    EmergencyContactName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PhysicalAddress = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewMembers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewMembers_AspNetUsers_ApplicationUserId",
                        column: x => x.ApplicationUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CrewMembers_BunkerOperators_EmployerOperatorId",
                        column: x => x.EmployerOperatorId,
                        principalTable: "BunkerOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BunkerBargeDeployments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BunkerBargeId = table.Column<int>(type: "int", nullable: false),
                    AreaOfOperationId = table.Column<int>(type: "int", nullable: false),
                    DeployedFrom = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DeployedTo = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkerBargeDeployments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BunkerBargeDeployments_BunkerAreasOfOperation_AreaOfOperationId",
                        column: x => x.AreaOfOperationId,
                        principalTable: "BunkerAreasOfOperation",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BunkerBargeDeployments_BunkerBarges_BunkerBargeId",
                        column: x => x.BunkerBargeId,
                        principalTable: "BunkerBarges",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BunkeringOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BunkerVesselId = table.Column<int>(type: "int", nullable: false),
                    CustomerVesselId = table.Column<int>(type: "int", nullable: false),
                    BunkerFuelId = table.Column<int>(type: "int", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlongsideTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CastOffTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JobStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JobEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PumpingStopTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkeringOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BunkeringOperations_BunkerFuels_BunkerFuelId",
                        column: x => x.BunkerFuelId,
                        principalTable: "BunkerFuels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BunkeringOperations_Vessels_BunkerVesselId",
                        column: x => x.BunkerVesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BunkeringOperations_Vessels_CustomerVesselId",
                        column: x => x.CustomerVesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "VesselComplianceSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VesselId = table.Column<int>(type: "int", nullable: false),
                    EvaluatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    CompliantCrewCount = table.Column<int>(type: "int", nullable: false),
                    NonCompliantCrewCount = table.Column<int>(type: "int", nullable: false),
                    ExpiringSoonCount = table.Column<int>(type: "int", nullable: false),
                    TotalCrewCount = table.Column<int>(type: "int", nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    EvaluatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselComplianceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VesselComplianceSnapshots_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrewDocuments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrewMemberId = table.Column<int>(type: "int", nullable: false),
                    DocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    IssuingAuthority = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IssuingCountry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DocumentNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IssuedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StorageUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ContentType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewDocuments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewDocuments_CrewMembers_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OperatorBillingAccountId = table.Column<int>(type: "int", nullable: true),
                    BunkerOperatorId = table.Column<int>(type: "int", nullable: false),
                    CrewMemberId = table.Column<int>(type: "int", nullable: true),
                    VesselId = table.Column<int>(type: "int", nullable: true),
                    InvoiceDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Currency = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SubTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_BunkerOperators_BunkerOperatorId",
                        column: x => x.BunkerOperatorId,
                        principalTable: "BunkerOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_CrewMembers_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_OperatorBillingAccounts_OperatorBillingAccountId",
                        column: x => x.OperatorBillingAccountId,
                        principalTable: "OperatorBillingAccounts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Invoices_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "BunkerBargeDeploymentAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BargeDeploymentId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    PerformedBy = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkerBargeDeploymentAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BunkerBargeDeploymentAudits_BunkerBargeDeployments_BargeDeploymentId",
                        column: x => x.BargeDeploymentId,
                        principalTable: "BunkerBargeDeployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BunkeringOperationPumpingIntervals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BunkeringOperationId = table.Column<int>(type: "int", nullable: false),
                    PumpStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PumpEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BunkerFuelId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    WindSpeed = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    WindDirection = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SeaState = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Swell = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Visibility = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkeringOperationPumpingIntervals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BunkeringOperationPumpingIntervals_BunkerFuels_BunkerFuelId",
                        column: x => x.BunkerFuelId,
                        principalTable: "BunkerFuels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BunkeringOperationPumpingIntervals_BunkeringOperations_BunkeringOperationId",
                        column: x => x.BunkeringOperationId,
                        principalTable: "BunkeringOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ISGOTTChecklistStageResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BunkeringOperationId = table.Column<int>(type: "int", nullable: false),
                    StageTemplateId = table.Column<int>(type: "int", nullable: false),
                    CompletedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISGOTTChecklistStageResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ISGOTTChecklistStageResponses_BunkeringOperations_BunkeringOperationId",
                        column: x => x.BunkeringOperationId,
                        principalTable: "BunkeringOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ISGOTTChecklistStageResponses_ISGOTTStageTemplates_StageTemplateId",
                        column: x => x.StageTemplateId,
                        principalTable: "ISGOTTStageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OilSpillIncidents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BunkeringOperationId = table.Column<int>(type: "int", nullable: true),
                    SpillName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    SpillStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SpillEndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ProductType = table.Column<int>(type: "int", nullable: false),
                    EstimatedVolume = table.Column<double>(type: "float", nullable: false),
                    ReleaseRate = table.Column<double>(type: "float", nullable: true),
                    SourceType = table.Column<int>(type: "int", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Commander = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilSpillIncidents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OilSpillIncidents_BunkeringOperations_BunkeringOperationId",
                        column: x => x.BunkeringOperationId,
                        principalTable: "BunkeringOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceResults",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrewMemberId = table.Column<int>(type: "int", nullable: false),
                    VesselId = table.Column<int>(type: "int", nullable: true),
                    EvaluatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    State = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    MissingRuleIds = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    AssignedCourseIds = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    TriggeredTrainingAssignment = table.Column<bool>(type: "bit", nullable: false),
                    TriggeredInvoiceGeneration = table.Column<bool>(type: "bit", nullable: false),
                    GeneratedInvoiceId = table.Column<int>(type: "int", nullable: true),
                    EvaluatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceResults", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceResults_CrewMembers_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ComplianceResults_Invoices_GeneratedInvoiceId",
                        column: x => x.GeneratedInvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComplianceResults_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "ISGOTTChecklistItemResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageResponseId = table.Column<int>(type: "int", nullable: false),
                    ItemTemplateId = table.Column<int>(type: "int", nullable: false),
                    YesNoNaValue = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TextValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISGOTTChecklistItemResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ISGOTTChecklistItemResponses_ISGOTTChecklistStageResponses_StageResponseId",
                        column: x => x.StageResponseId,
                        principalTable: "ISGOTTChecklistStageResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ISGOTTChecklistItemResponses_ISGOTTItemTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ISGOTTItemTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "IncidentForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    FormType = table.Column<int>(type: "int", nullable: false),
                    OperationalPeriodId = table.Column<int>(type: "int", nullable: true),
                    PersonName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    JsonData = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    IsLinkedToSitrep = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExportFilePath = table.Column<string>(type: "nvarchar(400)", maxLength: 400, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_IncidentForms", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IncidentForms_OilSpillIncidents_IncidentId",
                        column: x => x.IncidentId,
                        principalTable: "OilSpillIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OilSpillModelRuns",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpillId = table.Column<int>(type: "int", nullable: false),
                    RunName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DurationHours = table.Column<double>(type: "float", nullable: false),
                    TimeStepMinutes = table.Column<int>(type: "int", nullable: false),
                    WindSpeed = table.Column<double>(type: "float", nullable: false),
                    WindDirection = table.Column<double>(type: "float", nullable: false),
                    CurrentSpeed = table.Column<double>(type: "float", nullable: false),
                    CurrentDirection = table.Column<double>(type: "float", nullable: false),
                    TideState = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ShorelineImpactIndex = table.Column<int>(type: "int", nullable: true),
                    ShorelineImpactTime = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilSpillModelRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OilSpillModelRuns_OilSpillIncidents_SpillId",
                        column: x => x.SpillId,
                        principalTable: "OilSpillIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "OilSpillResponseActions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    SpillId = table.Column<int>(type: "int", nullable: false),
                    ActionType = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Latitude = table.Column<double>(type: "float", nullable: true),
                    Longitude = table.Column<double>(type: "float", nullable: true),
                    GeometryGeoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    RadiusMeters = table.Column<double>(type: "float", nullable: true),
                    PerformedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilSpillResponseActions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OilSpillResponseActions_OilSpillIncidents_SpillId",
                        column: x => x.SpillId,
                        principalTable: "OilSpillIncidents",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ComplianceAuditEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Action = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CrewMemberId = table.Column<int>(type: "int", nullable: true),
                    VesselId = table.Column<int>(type: "int", nullable: true),
                    ComplianceResultId = table.Column<int>(type: "int", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PayloadJson = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    OccurredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PerformedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    PerformedByDisplayName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceAuditEntries_ComplianceResults_ComplianceResultId",
                        column: x => x.ComplianceResultId,
                        principalTable: "ComplianceResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComplianceAuditEntries_CrewMembers_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ComplianceAuditEntries_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CrewAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrewMemberId = table.Column<int>(type: "int", nullable: false),
                    VesselId = table.Column<int>(type: "int", nullable: false),
                    RankOnAssignment = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SignOnDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SignOffDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PortOfSignOn = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    PortOfSignOff = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovingComplianceResultId = table.Column<int>(type: "int", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewAssignments_ComplianceResults_ApprovingComplianceResultId",
                        column: x => x.ApprovingComplianceResultId,
                        principalTable: "ComplianceResults",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CrewAssignments_CrewMembers_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CrewAssignments_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OilSpillTrajectoryPoints",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ModelRunId = table.Column<int>(type: "int", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Latitude = table.Column<double>(type: "float", nullable: false),
                    Longitude = table.Column<double>(type: "float", nullable: false),
                    AreaSqM = table.Column<double>(type: "float", nullable: false),
                    ThicknessMm = table.Column<double>(type: "float", nullable: true),
                    PolygonGeoJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CumulativePolygonGeoJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OilSpillTrajectoryPoints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OilSpillTrajectoryPoints_OilSpillModelRuns_ModelRunId",
                        column: x => x.ModelRunId,
                        principalTable: "OilSpillModelRuns",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentAttempts",
                columns: table => new
                {
                    AssessmentAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    StartedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScorePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Passed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentAttempts", x => x.AssessmentAttemptId);
                    table.ForeignKey(
                        name: "FK_AssessmentAttempts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentOptions",
                columns: table => new
                {
                    AssessmentOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentOptions", x => x.AssessmentOptionId);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentQuestions",
                columns: table => new
                {
                    AssessmentQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    PromptMarkdown = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ExplanationMarkdown = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 1m)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentQuestions", x => x.AssessmentQuestionId);
                });

            migrationBuilder.CreateTable(
                name: "AssessmentResponses",
                columns: table => new
                {
                    AssessmentResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessmentQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FreeTextAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    AwardedPoints = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AssessmentResponses", x => x.AssessmentResponseId);
                    table.ForeignKey(
                        name: "FK_AssessmentResponses_AssessmentAttempts_AssessmentAttemptId",
                        column: x => x.AssessmentAttemptId,
                        principalTable: "AssessmentAttempts",
                        principalColumn: "AssessmentAttemptId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AssessmentResponses_AssessmentOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "AssessmentOptions",
                        principalColumn: "AssessmentOptionId");
                    table.ForeignKey(
                        name: "FK_AssessmentResponses_AssessmentQuestions_AssessmentQuestionId",
                        column: x => x.AssessmentQuestionId,
                        principalTable: "AssessmentQuestions",
                        principalColumn: "AssessmentQuestionId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Assessments",
                columns: table => new
                {
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PassMarkPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 80m),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    RandomizeQuestions = table.Column<bool>(type: "bit", nullable: false),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: true),
                    ShowFeedbackAfterSubmit = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Assessments", x => x.AssessmentId);
                });

            migrationBuilder.CreateTable(
                name: "CertificateExpiryEvents",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrewMemberId = table.Column<int>(type: "int", nullable: false),
                    Source = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false),
                    TrainingCertificateId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CrewDocumentId = table.Column<int>(type: "int", nullable: true),
                    ExpiryDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DaysUntilExpiry = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    DetectedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NotifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AcknowledgedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificateExpiryEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CertificateExpiryEvents_CrewDocuments_CrewDocumentId",
                        column: x => x.CrewDocumentId,
                        principalTable: "CrewDocuments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CertificateExpiryEvents_CrewMembers_CrewMemberId",
                        column: x => x.CrewMemberId,
                        principalTable: "CrewMembers",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ComplianceRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Scope = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    RequiredForRank = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RequiredForVesselType = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    RequiredForOperatorId = table.Column<int>(type: "int", nullable: true),
                    RequiredCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequiredDocumentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CertificateValidityMonths = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ComplianceRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ComplianceRules_BunkerOperators_RequiredForOperatorId",
                        column: x => x.RequiredForOperatorId,
                        principalTable: "BunkerOperators",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "CourseAudienceRules",
                columns: table => new
                {
                    CourseAudienceRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RuleType = table.Column<int>(type: "int", nullable: false),
                    ApplicationRoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    OnBoardRole = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Qualification = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseAudienceRules", x => x.CourseAudienceRuleId);
                });

            migrationBuilder.CreateTable(
                name: "CourseCompletionRecords",
                columns: table => new
                {
                    CourseCompletionRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CompletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDateUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FinalScorePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCompletionRecords", x => x.CourseCompletionRecordId);
                    table.ForeignKey(
                        name: "FK_CourseCompletionRecords_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingCertificates",
                columns: table => new
                {
                    TrainingCertificateId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseCompletionRecordId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificateNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    VerificationCode = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    IssuedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiresOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RevokedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingCertificates", x => x.TrainingCertificateId);
                    table.ForeignKey(
                        name: "FK_TrainingCertificates_CourseCompletionRecords_CourseCompletionRecordId",
                        column: x => x.CourseCompletionRecordId,
                        principalTable: "CourseCompletionRecords",
                        principalColumn: "CourseCompletionRecordId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Courses",
                columns: table => new
                {
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PassMarkPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 80m),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TargetAudienceSummary = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    RegulatoryReference = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: true),
                    LearningObjectives = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ValidityMonths = table.Column<int>(type: "int", nullable: false, defaultValue: 12),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    EstimatedDurationMinutes = table.Column<int>(type: "int", nullable: true),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    CurrentVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Courses", x => x.CourseId);
                });

            migrationBuilder.CreateTable(
                name: "CourseVersions",
                columns: table => new
                {
                    CourseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VersionNumber = table.Column<int>(type: "int", nullable: false),
                    VersionLabel = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    EffectiveToUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ChangeSummary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseVersions", x => x.CourseVersionId);
                    table.ForeignKey(
                        name: "FK_CourseVersions_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLineItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    InvoiceId = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    LineTotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    RelatedCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Courses_RelatedCourseId",
                        column: x => x.RelatedCourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingCourseAssessments",
                columns: table => new
                {
                    TrainingCourseAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PassMarkPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 80m),
                    RandomQuestionCount = table.Column<int>(type: "int", nullable: false, defaultValue: 25),
                    MaxAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3),
                    TimeLimitMinutes = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingCourseAssessments", x => x.TrainingCourseAssessmentId);
                    table.ForeignKey(
                        name: "FK_TrainingCourseAssessments_Courses_TrainingCourseId",
                        column: x => x.TrainingCourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId");
                });

            migrationBuilder.CreateTable(
                name: "UserTrainingAssignments",
                columns: table => new
                {
                    UserTrainingAssignmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    AssignedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Reason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    RegisteredByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    RegisteredOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    ApprovedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ApprovalNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    InvoiceId = table.Column<int>(type: "int", nullable: true),
                    CompletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletionScorePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    PaidOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaidByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrainingAssignments", x => x.UserTrainingAssignmentId);
                    table.ForeignKey(
                        name: "FK_UserTrainingAssignments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTrainingAssignments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserTrainingAssignments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Modules",
                columns: table => new
                {
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CourseVersionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    HasModuleAssessment = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    AssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    AssessmentPassMarkPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    AssessmentMaxAttempts = table.Column<int>(type: "int", nullable: false, defaultValue: 3)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Modules", x => x.ModuleId);
                    table.ForeignKey(
                        name: "FK_Modules_CourseVersions_CourseVersionId",
                        column: x => x.CourseVersionId,
                        principalTable: "CourseVersions",
                        principalColumn: "CourseVersionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Modules_TrainingCourseAssessments_AssessmentId",
                        column: x => x.AssessmentId,
                        principalTable: "TrainingCourseAssessments",
                        principalColumn: "TrainingCourseAssessmentId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "UserAssessmentAttempts",
                columns: table => new
                {
                    UserAssessmentAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    StartedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SubmittedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ScorePercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    Passed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssessmentAttempts", x => x.UserAssessmentAttemptId);
                    table.ForeignKey(
                        name: "FK_UserAssessmentAttempts_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAssessmentAttempts_TrainingCourseAssessments_TrainingCourseAssessmentId",
                        column: x => x.TrainingCourseAssessmentId,
                        principalTable: "TrainingCourseAssessments",
                        principalColumn: "TrainingCourseAssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Lessons",
                columns: table => new
                {
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    EstimatedMinutes = table.Column<int>(type: "int", nullable: true),
                    IsPreview = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lessons", x => x.LessonId);
                    table.ForeignKey(
                        name: "FK_Lessons_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingQuestionBankQuestions",
                columns: table => new
                {
                    TrainingQuestionBankQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingCourseAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ScenarioText = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DifficultyLevel = table.Column<int>(type: "int", nullable: true),
                    Points = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 1m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingQuestionBankQuestions", x => x.TrainingQuestionBankQuestionId);
                    table.ForeignKey(
                        name: "FK_TrainingQuestionBankQuestions_Modules_TrainingModuleId",
                        column: x => x.TrainingModuleId,
                        principalTable: "Modules",
                        principalColumn: "ModuleId",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TrainingQuestionBankQuestions_TrainingCourseAssessments_TrainingCourseAssessmentId",
                        column: x => x.TrainingCourseAssessmentId,
                        principalTable: "TrainingCourseAssessments",
                        principalColumn: "TrainingCourseAssessmentId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "LessonBlocks",
                columns: table => new
                {
                    LessonBlockId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BlockType = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Subtitle = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    MarkdownBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ThumbnailUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    FileUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    ExternalUrl = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    MimeType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DurationSeconds = table.Column<int>(type: "int", nullable: true),
                    MetadataJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MediaAssetId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IsRequired = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LessonBlocks", x => x.LessonBlockId);
                    table.ForeignKey(
                        name: "FK_LessonBlocks_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "LessonId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LessonBlocks_MediaAssets_MediaAssetId",
                        column: x => x.MediaAssetId,
                        principalTable: "MediaAssets",
                        principalColumn: "MediaAssetId",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TrainingKnowledgeCheckQuestions",
                columns: table => new
                {
                    TrainingKnowledgeCheckQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingLessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    QuestionType = table.Column<int>(type: "int", nullable: false),
                    Prompt = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Explanation = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    OrderIndex = table.Column<int>(type: "int", nullable: false),
                    Points = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: false, defaultValue: 1m),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingKnowledgeCheckQuestions", x => x.TrainingKnowledgeCheckQuestionId);
                    table.ForeignKey(
                        name: "FK_TrainingKnowledgeCheckQuestions_Lessons_TrainingLessonId",
                        column: x => x.TrainingLessonId,
                        principalTable: "Lessons",
                        principalColumn: "LessonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserCourseProgress",
                columns: table => new
                {
                    UserCourseProgressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CourseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    AssignedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    StartedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAccessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PercentComplete = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    CurrentLessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ExpiryDateUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserCourseProgress", x => x.UserCourseProgressId);
                    table.ForeignKey(
                        name: "FK_UserCourseProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCourseProgress_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserCourseProgress_Lessons_CurrentLessonId",
                        column: x => x.CurrentLessonId,
                        principalTable: "Lessons",
                        principalColumn: "LessonId");
                });

            migrationBuilder.CreateTable(
                name: "UserLessonProgress",
                columns: table => new
                {
                    UserLessonProgressId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    LessonId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastAccessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PercentComplete = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    VideoSecondsWatched = table.Column<int>(type: "int", nullable: true),
                    ScrollPercent = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    CompletionEvidenceJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    KnowledgeCheckPassed = table.Column<bool>(type: "bit", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserLessonProgress", x => x.UserLessonProgressId);
                    table.ForeignKey(
                        name: "FK_UserLessonProgress_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserLessonProgress_Lessons_LessonId",
                        column: x => x.LessonId,
                        principalTable: "Lessons",
                        principalColumn: "LessonId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingQuestionBankOptions",
                columns: table => new
                {
                    TrainingQuestionBankOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingQuestionBankQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingQuestionBankOptions", x => x.TrainingQuestionBankOptionId);
                    table.ForeignKey(
                        name: "FK_TrainingQuestionBankOptions_TrainingQuestionBankQuestions_TrainingQuestionBankQuestionId",
                        column: x => x.TrainingQuestionBankQuestionId,
                        principalTable: "TrainingQuestionBankQuestions",
                        principalColumn: "TrainingQuestionBankQuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrainingKnowledgeCheckOptions",
                columns: table => new
                {
                    TrainingKnowledgeCheckOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingKnowledgeCheckQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    OptionText = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    OrderIndex = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrainingKnowledgeCheckOptions", x => x.TrainingKnowledgeCheckOptionId);
                    table.ForeignKey(
                        name: "FK_TrainingKnowledgeCheckOptions_TrainingKnowledgeCheckQuestions_TrainingKnowledgeCheckQuestionId",
                        column: x => x.TrainingKnowledgeCheckQuestionId,
                        principalTable: "TrainingKnowledgeCheckQuestions",
                        principalColumn: "TrainingKnowledgeCheckQuestionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAssessmentResponses",
                columns: table => new
                {
                    UserAssessmentResponseId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserAssessmentAttemptId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TrainingQuestionBankQuestionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SelectedOptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FreeTextAnswer = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IsCorrect = table.Column<bool>(type: "bit", nullable: true),
                    AwardedPoints = table.Column<decimal>(type: "decimal(8,2)", precision: 8, scale: 2, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAssessmentResponses", x => x.UserAssessmentResponseId);
                    table.ForeignKey(
                        name: "FK_UserAssessmentResponses_TrainingQuestionBankOptions_SelectedOptionId",
                        column: x => x.SelectedOptionId,
                        principalTable: "TrainingQuestionBankOptions",
                        principalColumn: "TrainingQuestionBankOptionId");
                    table.ForeignKey(
                        name: "FK_UserAssessmentResponses_TrainingQuestionBankQuestions_TrainingQuestionBankQuestionId",
                        column: x => x.TrainingQuestionBankQuestionId,
                        principalTable: "TrainingQuestionBankQuestions",
                        principalColumn: "TrainingQuestionBankQuestionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserAssessmentResponses_UserAssessmentAttempts_UserAssessmentAttemptId",
                        column: x => x.UserAssessmentAttemptId,
                        principalTable: "UserAssessmentAttempts",
                        principalColumn: "UserAssessmentAttemptId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserPasskeys_UserId",
                table: "AspNetUserPasskeys",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempts_AssessmentId_UserId_AttemptNumber",
                table: "AssessmentAttempts",
                columns: new[] { "AssessmentId", "UserId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentAttempts_UserId",
                table: "AssessmentAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentOptions_AssessmentQuestionId_OrderIndex",
                table: "AssessmentOptions",
                columns: new[] { "AssessmentQuestionId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentQuestions_AssessmentId_OrderIndex",
                table: "AssessmentQuestions",
                columns: new[] { "AssessmentId", "OrderIndex" });

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResponses_AssessmentAttemptId",
                table: "AssessmentResponses",
                column: "AssessmentAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResponses_AssessmentQuestionId",
                table: "AssessmentResponses",
                column: "AssessmentQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_AssessmentResponses_SelectedOptionId",
                table: "AssessmentResponses",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Assessments_LessonId",
                table: "Assessments",
                column: "LessonId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BunkerAreasOfOperation_Name",
                table: "BunkerAreasOfOperation",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BunkerBargeDeploymentAudits_BargeDeploymentId_Timestamp",
                table: "BunkerBargeDeploymentAudits",
                columns: new[] { "BargeDeploymentId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_BunkerBargeDeployments_AreaOfOperationId_DeployedFrom_DeployedTo",
                table: "BunkerBargeDeployments",
                columns: new[] { "AreaOfOperationId", "DeployedFrom", "DeployedTo" });

            migrationBuilder.CreateIndex(
                name: "IX_BunkerBargeDeployments_BunkerBargeId_DeployedFrom_DeployedTo",
                table: "BunkerBargeDeployments",
                columns: new[] { "BunkerBargeId", "DeployedFrom", "DeployedTo" });

            migrationBuilder.CreateIndex(
                name: "IX_BunkerBarges_BunkerOperatorId",
                table: "BunkerBarges",
                column: "BunkerOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkerBarges_Name",
                table: "BunkerBarges",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_BunkerFuels_Code",
                table: "BunkerFuels",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringOperationPumpingIntervals_BunkerFuelId",
                table: "BunkeringOperationPumpingIntervals",
                column: "BunkerFuelId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringOperationPumpingIntervals_BunkeringOperationId",
                table: "BunkeringOperationPumpingIntervals",
                column: "BunkeringOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringOperations_BunkerFuelId",
                table: "BunkeringOperations",
                column: "BunkerFuelId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringOperations_BunkerVesselId",
                table: "BunkeringOperations",
                column: "BunkerVesselId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringOperations_CustomerVesselId",
                table: "BunkeringOperations",
                column: "CustomerVesselId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkerOperatorAreaAssignments_AreaOfOperationId",
                table: "BunkerOperatorAreaAssignments",
                column: "AreaOfOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkerOperatorAreaAssignments_BunkerOperatorId_AreaOfOperationId_AssignedFrom_AssignedTo",
                table: "BunkerOperatorAreaAssignments",
                columns: new[] { "BunkerOperatorId", "AreaOfOperationId", "AssignedFrom", "AssignedTo" });

            migrationBuilder.CreateIndex(
                name: "IX_BunkerOperators_Name",
                table: "BunkerOperators",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BunkerPorts_AreaOfOperationId",
                table: "BunkerPorts",
                column: "AreaOfOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkerPorts_AreaOfOperationId_Name",
                table: "BunkerPorts",
                columns: new[] { "AreaOfOperationId", "Name" });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateExpiryEvents_CrewDocumentId",
                table: "CertificateExpiryEvents",
                column: "CrewDocumentId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateExpiryEvents_CrewMemberId",
                table: "CertificateExpiryEvents",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificateExpiryEvents_Status_ExpiryDateUtc",
                table: "CertificateExpiryEvents",
                columns: new[] { "Status", "ExpiryDateUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_CertificateExpiryEvents_TrainingCertificateId",
                table: "CertificateExpiryEvents",
                column: "TrainingCertificateId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceAuditEntries_Action_OccurredOnUtc",
                table: "ComplianceAuditEntries",
                columns: new[] { "Action", "OccurredOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceAuditEntries_ComplianceResultId",
                table: "ComplianceAuditEntries",
                column: "ComplianceResultId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceAuditEntries_CrewMemberId",
                table: "ComplianceAuditEntries",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceAuditEntries_VesselId",
                table: "ComplianceAuditEntries",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceResults_CrewMemberId_EvaluatedOnUtc",
                table: "ComplianceResults",
                columns: new[] { "CrewMemberId", "EvaluatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceResults_GeneratedInvoiceId",
                table: "ComplianceResults",
                column: "GeneratedInvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceResults_VesselId",
                table: "ComplianceResults",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRules_RequiredCourseId",
                table: "ComplianceRules",
                column: "RequiredCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRules_RequiredForOperatorId",
                table: "ComplianceRules",
                column: "RequiredForOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_ComplianceRules_Scope_IsActive",
                table: "ComplianceRules",
                columns: new[] { "Scope", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_CourseAudienceRules_CourseId",
                table: "CourseAudienceRules",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseCompletionRecords_CertificateNumber",
                table: "CourseCompletionRecords",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseCompletionRecords_CourseId",
                table: "CourseCompletionRecords",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseCompletionRecords_CourseVersionId",
                table: "CourseCompletionRecords",
                column: "CourseVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseCompletionRecords_UserId",
                table: "CourseCompletionRecords",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Courses_Code",
                table: "Courses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Courses_CurrentVersionId",
                table: "Courses",
                column: "CurrentVersionId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseVersions_CourseId_VersionNumber",
                table: "CourseVersions",
                columns: new[] { "CourseId", "VersionNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CrewAssignments_ApprovingComplianceResultId",
                table: "CrewAssignments",
                column: "ApprovingComplianceResultId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewAssignments_CrewMemberId_Status",
                table: "CrewAssignments",
                columns: new[] { "CrewMemberId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CrewAssignments_SignOnDateUtc",
                table: "CrewAssignments",
                column: "SignOnDateUtc");

            migrationBuilder.CreateIndex(
                name: "IX_CrewAssignments_VesselId_Status",
                table: "CrewAssignments",
                columns: new[] { "VesselId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CrewDocuments_CrewMemberId_DocumentType",
                table: "CrewDocuments",
                columns: new[] { "CrewMemberId", "DocumentType" });

            migrationBuilder.CreateIndex(
                name: "IX_CrewDocuments_ExpiryDate",
                table: "CrewDocuments",
                column: "ExpiryDate");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_ApplicationUserId",
                table: "CrewMembers",
                column: "ApplicationUserId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_EmployerOperatorId",
                table: "CrewMembers",
                column: "EmployerOperatorId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_LastName_FirstName",
                table: "CrewMembers",
                columns: new[] { "LastName", "FirstName" });

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_PassportNumber",
                table: "CrewMembers",
                column: "PassportNumber");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMembers_SidNumber",
                table: "CrewMembers",
                column: "SidNumber");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentForms_IncidentId",
                table: "IncidentForms",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentForms_IncidentId_FormType",
                table: "IncidentForms",
                columns: new[] { "IncidentId", "FormType" });

            migrationBuilder.CreateIndex(
                name: "IX_IncidentForms_IncidentId_FormType_OperationalPeriodId",
                table: "IncidentForms",
                columns: new[] { "IncidentId", "FormType", "OperationalPeriodId" });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_InvoiceId",
                table: "InvoiceLineItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_RelatedCourseId",
                table: "InvoiceLineItems",
                column: "RelatedCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_BunkerOperatorId_Status",
                table: "Invoices",
                columns: new[] { "BunkerOperatorId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_CrewMemberId",
                table: "Invoices",
                column: "CrewMemberId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_OperatorBillingAccountId",
                table: "Invoices",
                column: "OperatorBillingAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_VesselId",
                table: "Invoices",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_ISGOTTChecklistItemResponses_ItemTemplateId",
                table: "ISGOTTChecklistItemResponses",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ISGOTTChecklistItemResponses_StageResponseId",
                table: "ISGOTTChecklistItemResponses",
                column: "StageResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_ISGOTTChecklistStageResponses_BunkeringOperationId",
                table: "ISGOTTChecklistStageResponses",
                column: "BunkeringOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ISGOTTChecklistStageResponses_StageTemplateId",
                table: "ISGOTTChecklistStageResponses",
                column: "StageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ISGOTTItemTemplates_StageTemplateId",
                table: "ISGOTTItemTemplates",
                column: "StageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonBlocks_LessonId_OrderIndex",
                table: "LessonBlocks",
                columns: new[] { "LessonId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_LessonBlocks_MediaAssetId",
                table: "LessonBlocks",
                column: "MediaAssetId");

            migrationBuilder.CreateIndex(
                name: "IX_LessonContentItems_LessonId_ContentType",
                table: "LessonContentItems",
                columns: new[] { "LessonId", "ContentType" });

            migrationBuilder.CreateIndex(
                name: "IX_Lessons_ModuleId_OrderIndex",
                table: "Lessons",
                columns: new[] { "ModuleId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Modules_AssessmentId",
                table: "Modules",
                column: "AssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_Modules_CourseVersionId_OrderIndex",
                table: "Modules",
                columns: new[] { "CourseVersionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_CreatedOnUtc",
                table: "NotificationMessages",
                column: "CreatedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_RecipientUserId_Status",
                table: "NotificationMessages",
                columns: new[] { "RecipientUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OilSpillIncidents_BunkeringOperationId",
                table: "OilSpillIncidents",
                column: "BunkeringOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_OilSpillIncidents_Status",
                table: "OilSpillIncidents",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OilSpillModelRuns_SpillId",
                table: "OilSpillModelRuns",
                column: "SpillId");

            migrationBuilder.CreateIndex(
                name: "IX_OilSpillResponseActions_SpillId",
                table: "OilSpillResponseActions",
                column: "SpillId");

            migrationBuilder.CreateIndex(
                name: "IX_OilSpillTrajectoryPoints_ModelRunId_Timestamp",
                table: "OilSpillTrajectoryPoints",
                columns: new[] { "ModelRunId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorBillingAccounts_BunkerOperatorId",
                table: "OperatorBillingAccounts",
                column: "BunkerOperatorId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertificates_CertificateNumber",
                table: "TrainingCertificates",
                column: "CertificateNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertificates_CourseCompletionRecordId",
                table: "TrainingCertificates",
                column: "CourseCompletionRecordId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCertificates_VerificationCode",
                table: "TrainingCertificates",
                column: "VerificationCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingCourseAssessments_TrainingCourseId",
                table: "TrainingCourseAssessments",
                column: "TrainingCourseId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingKnowledgeCheckOptions_TrainingKnowledgeCheckQuestionId_OrderIndex",
                table: "TrainingKnowledgeCheckOptions",
                columns: new[] { "TrainingKnowledgeCheckQuestionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingKnowledgeCheckQuestions_TrainingLessonId_OrderIndex",
                table: "TrainingKnowledgeCheckQuestions",
                columns: new[] { "TrainingLessonId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionBankOptions_TrainingQuestionBankQuestionId_OrderIndex",
                table: "TrainingQuestionBankOptions",
                columns: new[] { "TrainingQuestionBankQuestionId", "OrderIndex" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionBankQuestions_TrainingCourseAssessmentId",
                table: "TrainingQuestionBankQuestions",
                column: "TrainingCourseAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_TrainingQuestionBankQuestions_TrainingModuleId",
                table: "TrainingQuestionBankQuestions",
                column: "TrainingModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAttempts_TrainingCourseAssessmentId_UserId_AttemptNumber",
                table: "UserAssessmentAttempts",
                columns: new[] { "TrainingCourseAssessmentId", "UserId", "AttemptNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentAttempts_UserId",
                table: "UserAssessmentAttempts",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentResponses_SelectedOptionId",
                table: "UserAssessmentResponses",
                column: "SelectedOptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentResponses_TrainingQuestionBankQuestionId",
                table: "UserAssessmentResponses",
                column: "TrainingQuestionBankQuestionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAssessmentResponses_UserAssessmentAttemptId",
                table: "UserAssessmentResponses",
                column: "UserAssessmentAttemptId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCourseProgress_CourseId",
                table: "UserCourseProgress",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCourseProgress_CurrentLessonId",
                table: "UserCourseProgress",
                column: "CurrentLessonId");

            migrationBuilder.CreateIndex(
                name: "IX_UserCourseProgress_UserId_CourseId",
                table: "UserCourseProgress",
                columns: new[] { "UserId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserLessonProgress_LessonId",
                table: "UserLessonProgress",
                column: "LessonId");

            migrationBuilder.CreateIndex(
                name: "IX_UserLessonProgress_UserId_LessonId",
                table: "UserLessonProgress",
                columns: new[] { "UserId", "LessonId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_ApprovedOnUtc",
                table: "UserTrainingAssignments",
                column: "ApprovedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_CourseId",
                table: "UserTrainingAssignments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_InvoiceId",
                table: "UserTrainingAssignments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_Status",
                table: "UserTrainingAssignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_UserId_CourseId",
                table: "UserTrainingAssignments",
                columns: new[] { "UserId", "CourseId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_UserId_Status",
                table: "UserTrainingAssignments",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_VesselComplianceSnapshots_VesselId_EvaluatedOnUtc",
                table: "VesselComplianceSnapshots",
                columns: new[] { "VesselId", "EvaluatedOnUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_IMO",
                table: "Vessels",
                column: "IMO",
                unique: true,
                filter: "[IMO] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_Name",
                table: "Vessels",
                column: "Name");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_OwningOperatorId",
                table: "Vessels",
                column: "OwningOperatorId");

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentAttempts_Assessments_AssessmentId",
                table: "AssessmentAttempts",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentOptions_AssessmentQuestions_AssessmentQuestionId",
                table: "AssessmentOptions",
                column: "AssessmentQuestionId",
                principalTable: "AssessmentQuestions",
                principalColumn: "AssessmentQuestionId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AssessmentQuestions_Assessments_AssessmentId",
                table: "AssessmentQuestions",
                column: "AssessmentId",
                principalTable: "Assessments",
                principalColumn: "AssessmentId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Assessments_Lessons_LessonId",
                table: "Assessments",
                column: "LessonId",
                principalTable: "Lessons",
                principalColumn: "LessonId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CertificateExpiryEvents_TrainingCertificates_TrainingCertificateId",
                table: "CertificateExpiryEvents",
                column: "TrainingCertificateId",
                principalTable: "TrainingCertificates",
                principalColumn: "TrainingCertificateId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ComplianceRules_Courses_RequiredCourseId",
                table: "ComplianceRules",
                column: "RequiredCourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseAudienceRules_Courses_CourseId",
                table: "CourseAudienceRules",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseCompletionRecords_CourseVersions_CourseVersionId",
                table: "CourseCompletionRecords",
                column: "CourseVersionId",
                principalTable: "CourseVersions",
                principalColumn: "CourseVersionId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CourseCompletionRecords_Courses_CourseId",
                table: "CourseCompletionRecords",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "CourseId",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Courses_CourseVersions_CurrentVersionId",
                table: "Courses",
                column: "CurrentVersionId",
                principalTable: "CourseVersions",
                principalColumn: "CourseVersionId",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CourseVersions_Courses_CourseId",
                table: "CourseVersions");

            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserPasskeys");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AssessmentResponses");

            migrationBuilder.DropTable(
                name: "BunkerBargeDeploymentAudits");

            migrationBuilder.DropTable(
                name: "BunkeringOperationPumpingIntervals");

            migrationBuilder.DropTable(
                name: "BunkerOperatorAreaAssignments");

            migrationBuilder.DropTable(
                name: "BunkerPorts");

            migrationBuilder.DropTable(
                name: "CertificateExpiryEvents");

            migrationBuilder.DropTable(
                name: "ComplianceAuditEntries");

            migrationBuilder.DropTable(
                name: "ComplianceRules");

            migrationBuilder.DropTable(
                name: "CourseAudienceRules");

            migrationBuilder.DropTable(
                name: "CrewAssignments");

            migrationBuilder.DropTable(
                name: "IncidentForms");

            migrationBuilder.DropTable(
                name: "InvoiceLineItems");

            migrationBuilder.DropTable(
                name: "ISGOTTChecklistItemResponses");

            migrationBuilder.DropTable(
                name: "LessonBlocks");

            migrationBuilder.DropTable(
                name: "LessonContentItems");

            migrationBuilder.DropTable(
                name: "NotificationMessages");

            migrationBuilder.DropTable(
                name: "OilSpillResponseActions");

            migrationBuilder.DropTable(
                name: "OilSpillTrajectoryPoints");

            migrationBuilder.DropTable(
                name: "TrainingAuditLogs");

            migrationBuilder.DropTable(
                name: "TrainingKnowledgeCheckOptions");

            migrationBuilder.DropTable(
                name: "UserAssessmentResponses");

            migrationBuilder.DropTable(
                name: "UserCourseProgress");

            migrationBuilder.DropTable(
                name: "UserLessonProgress");

            migrationBuilder.DropTable(
                name: "UserTrainingAssignments");

            migrationBuilder.DropTable(
                name: "VesselComplianceSnapshots");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "AssessmentAttempts");

            migrationBuilder.DropTable(
                name: "AssessmentOptions");

            migrationBuilder.DropTable(
                name: "BunkerBargeDeployments");

            migrationBuilder.DropTable(
                name: "CrewDocuments");

            migrationBuilder.DropTable(
                name: "TrainingCertificates");

            migrationBuilder.DropTable(
                name: "ComplianceResults");

            migrationBuilder.DropTable(
                name: "ISGOTTChecklistStageResponses");

            migrationBuilder.DropTable(
                name: "ISGOTTItemTemplates");

            migrationBuilder.DropTable(
                name: "MediaAssets");

            migrationBuilder.DropTable(
                name: "AuthoringLessons");

            migrationBuilder.DropTable(
                name: "OilSpillModelRuns");

            migrationBuilder.DropTable(
                name: "TrainingKnowledgeCheckQuestions");

            migrationBuilder.DropTable(
                name: "TrainingQuestionBankOptions");

            migrationBuilder.DropTable(
                name: "UserAssessmentAttempts");

            migrationBuilder.DropTable(
                name: "AssessmentQuestions");

            migrationBuilder.DropTable(
                name: "BunkerAreasOfOperation");

            migrationBuilder.DropTable(
                name: "BunkerBarges");

            migrationBuilder.DropTable(
                name: "CourseCompletionRecords");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "ISGOTTStageTemplates");

            migrationBuilder.DropTable(
                name: "OilSpillIncidents");

            migrationBuilder.DropTable(
                name: "TrainingQuestionBankQuestions");

            migrationBuilder.DropTable(
                name: "Assessments");

            migrationBuilder.DropTable(
                name: "CrewMembers");

            migrationBuilder.DropTable(
                name: "OperatorBillingAccounts");

            migrationBuilder.DropTable(
                name: "BunkeringOperations");

            migrationBuilder.DropTable(
                name: "Lessons");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "BunkerFuels");

            migrationBuilder.DropTable(
                name: "Vessels");

            migrationBuilder.DropTable(
                name: "Modules");

            migrationBuilder.DropTable(
                name: "BunkerOperators");

            migrationBuilder.DropTable(
                name: "TrainingCourseAssessments");

            migrationBuilder.DropTable(
                name: "Courses");

            migrationBuilder.DropTable(
                name: "CourseVersions");
        }
    }
}

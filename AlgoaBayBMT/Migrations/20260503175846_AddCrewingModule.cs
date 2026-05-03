using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewingModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    table.ForeignKey(
                        name: "FK_ComplianceRules_Courses_RequiredCourseId",
                        column: x => x.RequiredCourseId,
                        principalTable: "Courses",
                        principalColumn: "CourseId",
                        onDelete: ReferentialAction.SetNull);
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
                    LengthOverall = table.Column<double>(type: "float", nullable: true),
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
                    table.ForeignKey(
                        name: "FK_CertificateExpiryEvents_TrainingCertificates_TrainingCertificateId",
                        column: x => x.TrainingCertificateId,
                        principalTable: "TrainingCertificates",
                        principalColumn: "TrainingCertificateId",
                        onDelete: ReferentialAction.SetNull);
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
                name: "IX_NotificationMessages_CreatedOnUtc",
                table: "NotificationMessages",
                column: "CreatedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationMessages_RecipientUserId_Status",
                table: "NotificationMessages",
                columns: new[] { "RecipientUserId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_OperatorBillingAccounts_BunkerOperatorId",
                table: "OperatorBillingAccounts",
                column: "BunkerOperatorId",
                unique: true);

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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CertificateExpiryEvents");

            migrationBuilder.DropTable(
                name: "ComplianceAuditEntries");

            migrationBuilder.DropTable(
                name: "ComplianceRules");

            migrationBuilder.DropTable(
                name: "CrewAssignments");

            migrationBuilder.DropTable(
                name: "InvoiceLineItems");

            migrationBuilder.DropTable(
                name: "NotificationMessages");

            migrationBuilder.DropTable(
                name: "VesselComplianceSnapshots");

            migrationBuilder.DropTable(
                name: "CrewDocuments");

            migrationBuilder.DropTable(
                name: "ComplianceResults");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "CrewMembers");

            migrationBuilder.DropTable(
                name: "OperatorBillingAccounts");

            migrationBuilder.DropTable(
                name: "Vessels");
        }
    }
}

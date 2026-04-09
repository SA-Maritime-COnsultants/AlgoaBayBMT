using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserManagementDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "AspNetUsers",
                type: "nvarchar(1024)",
                maxLength: 1024,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ApprovalStatus",
                table: "AspNetUsers",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "AspNetUsers",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOnUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CompanyId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "AspNetUsers",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAccountApproved",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "PrimaryAreaId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredOnUtc",
                table: "AspNetUsers",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "RequestedRole",
                table: "AspNetUsers",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VesselId",
                table: "AspNetUsers",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "AuthorityContacts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorityRole = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    FullName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorityContacts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BunkeringCompanies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    RegistrationNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ContactEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkeringCompanies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OperationalAreas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OperationalAreas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Vessels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ImoNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CallSign = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FlagState = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Vessels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Vessels_BunkeringCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "BunkeringCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "AreaNotificationDistributionRules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationalAreaId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    AuthorityContactId = table.Column<int>(type: "int", nullable: true),
                    NotificationCategory = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RecipientRoleName = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AreaNotificationDistributionRules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AreaNotificationDistributionRules_AuthorityContacts_AuthorityContactId",
                        column: x => x.AuthorityContactId,
                        principalTable: "AuthorityContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AreaNotificationDistributionRules_BunkeringCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "BunkeringCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_AreaNotificationDistributionRules_OperationalAreas_OperationalAreaId",
                        column: x => x.OperationalAreaId,
                        principalTable: "OperationalAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuthorityAreaAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AuthorityContactId = table.Column<int>(type: "int", nullable: false),
                    OperationalAreaId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuthorityAreaAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuthorityAreaAssignments_AuthorityContacts_AuthorityContactId",
                        column: x => x.AuthorityContactId,
                        principalTable: "AuthorityContacts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AuthorityAreaAssignments_OperationalAreas_OperationalAreaId",
                        column: x => x.OperationalAreaId,
                        principalTable: "OperationalAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CompanyAreaAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    OperationalAreaId = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompanyAreaAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CompanyAreaAssignments_BunkeringCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "BunkeringCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CompanyAreaAssignments_OperationalAreas_OperationalAreaId",
                        column: x => x.OperationalAreaId,
                        principalTable: "OperationalAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ports",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationalAreaId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ports", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ports_OperationalAreas_OperationalAreaId",
                        column: x => x.OperationalAreaId,
                        principalTable: "OperationalAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAreaAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    OperationalAreaId = table.Column<int>(type: "int", nullable: false),
                    IsPrimary = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AssignedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAreaAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAreaAssignments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAreaAssignments_OperationalAreas_OperationalAreaId",
                        column: x => x.OperationalAreaId,
                        principalTable: "OperationalAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CrewDeployments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    VesselId = table.Column<int>(type: "int", nullable: false),
                    OnboardRoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OnboardRoleType = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    StartedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeployedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewDeployments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewDeployments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CrewDeployments_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VesselRoleAssignments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VesselId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: true),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    VesselRoleType = table.Column<int>(type: "int", nullable: false),
                    AssignedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AssignedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselRoleAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VesselRoleAssignments_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VesselRoleAssignments_BunkeringCompanies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "BunkeringCompanies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_VesselRoleAssignments_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Bays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationalAreaId = table.Column<int>(type: "int", nullable: false),
                    PortId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Bays_OperationalAreas_OperationalAreaId",
                        column: x => x.OperationalAreaId,
                        principalTable: "OperationalAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Bays_Ports_PortId",
                        column: x => x.PortId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateTable(
                name: "CrewDeploymentComplianceSnapshots",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CrewDeploymentId = table.Column<int>(type: "int", nullable: false),
                    ComplianceItemName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    ComplianceState = table.Column<int>(type: "int", nullable: false),
                    ExpiresOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    RecordedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewDeploymentComplianceSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewDeploymentComplianceSnapshots_CrewDeployments_CrewDeploymentId",
                        column: x => x.CrewDeploymentId,
                        principalTable: "CrewDeployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VesselCrewListEntries",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VesselId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CrewDeploymentId = table.Column<int>(type: "int", nullable: true),
                    OnboardRoleName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OnboardRoleType = table.Column<int>(type: "int", nullable: false),
                    JoinedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LeftOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ComplianceState = table.Column<int>(type: "int", nullable: false),
                    ComplianceExpiresOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCurrent = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VesselCrewListEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VesselCrewListEntries_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_VesselCrewListEntries_CrewDeployments_CrewDeploymentId",
                        column: x => x.CrewDeploymentId,
                        principalTable: "CrewDeployments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_VesselCrewListEntries_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Anchorages",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OperationalAreaId = table.Column<int>(type: "int", nullable: false),
                    PortId = table.Column<int>(type: "int", nullable: true),
                    BayId = table.Column<int>(type: "int", nullable: true),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Anchorages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Anchorages_Bays_BayId",
                        column: x => x.BayId,
                        principalTable: "Bays",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                    table.ForeignKey(
                        name: "FK_Anchorages_OperationalAreas_OperationalAreaId",
                        column: x => x.OperationalAreaId,
                        principalTable: "OperationalAreas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Anchorages_Ports_PortId",
                        column: x => x.PortId,
                        principalTable: "Ports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_PrimaryAreaId",
                table: "AspNetUsers",
                column: "PrimaryAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_VesselId",
                table: "AspNetUsers",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_Anchorages_BayId",
                table: "Anchorages",
                column: "BayId");

            migrationBuilder.CreateIndex(
                name: "IX_Anchorages_OperationalAreaId_Code",
                table: "Anchorages",
                columns: new[] { "OperationalAreaId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Anchorages_PortId",
                table: "Anchorages",
                column: "PortId");

            migrationBuilder.CreateIndex(
                name: "IX_AreaNotificationDistributionRules_AuthorityContactId",
                table: "AreaNotificationDistributionRules",
                column: "AuthorityContactId");

            migrationBuilder.CreateIndex(
                name: "IX_AreaNotificationDistributionRules_CompanyId",
                table: "AreaNotificationDistributionRules",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_AreaNotificationDistributionRules_OperationalAreaId",
                table: "AreaNotificationDistributionRules",
                column: "OperationalAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityAreaAssignments_AuthorityContactId_OperationalAreaId",
                table: "AuthorityAreaAssignments",
                columns: new[] { "AuthorityContactId", "OperationalAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AuthorityAreaAssignments_OperationalAreaId",
                table: "AuthorityAreaAssignments",
                column: "OperationalAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_Bays_OperationalAreaId_Code",
                table: "Bays",
                columns: new[] { "OperationalAreaId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Bays_PortId",
                table: "Bays",
                column: "PortId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringCompanies_RegistrationNumber",
                table: "BunkeringCompanies",
                column: "RegistrationNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAreaAssignments_CompanyId_OperationalAreaId",
                table: "CompanyAreaAssignments",
                columns: new[] { "CompanyId", "OperationalAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CompanyAreaAssignments_OperationalAreaId",
                table: "CompanyAreaAssignments",
                column: "OperationalAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewDeploymentComplianceSnapshots_CrewDeploymentId",
                table: "CrewDeploymentComplianceSnapshots",
                column: "CrewDeploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewDeployments_UserId_VesselId_Status",
                table: "CrewDeployments",
                columns: new[] { "UserId", "VesselId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CrewDeployments_VesselId",
                table: "CrewDeployments",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_OperationalAreas_Code",
                table: "OperationalAreas",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Ports_OperationalAreaId_Code",
                table: "Ports",
                columns: new[] { "OperationalAreaId", "Code" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserAreaAssignments_OperationalAreaId",
                table: "UserAreaAssignments",
                column: "OperationalAreaId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAreaAssignments_UserId_OperationalAreaId",
                table: "UserAreaAssignments",
                columns: new[] { "UserId", "OperationalAreaId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_VesselCrewListEntries_CrewDeploymentId",
                table: "VesselCrewListEntries",
                column: "CrewDeploymentId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCrewListEntries_UserId",
                table: "VesselCrewListEntries",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselCrewListEntries_VesselId_UserId_IsCurrent",
                table: "VesselCrewListEntries",
                columns: new[] { "VesselId", "UserId", "IsCurrent" });

            migrationBuilder.CreateIndex(
                name: "IX_VesselRoleAssignments_CompanyId",
                table: "VesselRoleAssignments",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_VesselRoleAssignments_UserId_VesselId_VesselRoleType_IsActive",
                table: "VesselRoleAssignments",
                columns: new[] { "UserId", "VesselId", "VesselRoleType", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_VesselRoleAssignments_VesselId",
                table: "VesselRoleAssignments",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_CompanyId",
                table: "Vessels",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_Vessels_ImoNumber",
                table: "Vessels",
                column: "ImoNumber",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_BunkeringCompanies_CompanyId",
                table: "AspNetUsers",
                column: "CompanyId",
                principalTable: "BunkeringCompanies",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_OperationalAreas_PrimaryAreaId",
                table: "AspNetUsers",
                column: "PrimaryAreaId",
                principalTable: "OperationalAreas",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Vessels_VesselId",
                table: "AspNetUsers",
                column: "VesselId",
                principalTable: "Vessels",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_BunkeringCompanies_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_OperationalAreas_PrimaryAreaId",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Vessels_VesselId",
                table: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Anchorages");

            migrationBuilder.DropTable(
                name: "AreaNotificationDistributionRules");

            migrationBuilder.DropTable(
                name: "AuthorityAreaAssignments");

            migrationBuilder.DropTable(
                name: "CompanyAreaAssignments");

            migrationBuilder.DropTable(
                name: "CrewDeploymentComplianceSnapshots");

            migrationBuilder.DropTable(
                name: "UserAreaAssignments");

            migrationBuilder.DropTable(
                name: "VesselCrewListEntries");

            migrationBuilder.DropTable(
                name: "VesselRoleAssignments");

            migrationBuilder.DropTable(
                name: "Bays");

            migrationBuilder.DropTable(
                name: "AuthorityContacts");

            migrationBuilder.DropTable(
                name: "CrewDeployments");

            migrationBuilder.DropTable(
                name: "Ports");

            migrationBuilder.DropTable(
                name: "Vessels");

            migrationBuilder.DropTable(
                name: "OperationalAreas");

            migrationBuilder.DropTable(
                name: "BunkeringCompanies");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_PrimaryAreaId",
                table: "AspNetUsers");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_VesselId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "ApprovedOnUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "CompanyId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsAccountApproved",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PrimaryAreaId",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RegisteredOnUtc",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "RequestedRole",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "VesselId",
                table: "AspNetUsers");
        }
    }
}

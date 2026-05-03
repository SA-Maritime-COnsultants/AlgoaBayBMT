using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    public partial class AddBargesModule : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BunkerBargeDeploymentAudits");

            migrationBuilder.DropTable(
                name: "BunkerOperatorAreaAssignments");

            migrationBuilder.DropTable(
                name: "BunkerPorts");

            migrationBuilder.DropTable(
                name: "BunkerBargeDeployments");

            migrationBuilder.DropTable(
                name: "BunkerAreasOfOperation");

            migrationBuilder.DropTable(
                name: "BunkerBarges");

            migrationBuilder.DropTable(
                name: "BunkerOperators");
        }
    }
}

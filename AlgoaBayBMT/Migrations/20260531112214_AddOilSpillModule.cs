using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class AddOilSpillModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
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
                    PolygonGeoJson = table.Column<string>(type: "nvarchar(max)", nullable: true)
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
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OilSpillResponseActions");

            migrationBuilder.DropTable(
                name: "OilSpillTrajectoryPoints");

            migrationBuilder.DropTable(
                name: "OilSpillModelRuns");

            migrationBuilder.DropTable(
                name: "OilSpillIncidents");
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class OilSpillDualScenario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Scenario",
                table: "OilSpillTrajectoryPoints",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "HasMitigatedScenario",
                table: "OilSpillModelRuns",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MitigatedShorelineImpactGeoJson",
                table: "OilSpillModelRuns",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MitigatedShorelineImpactIndex",
                table: "OilSpillModelRuns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MitigatedShorelineImpactTime",
                table: "OilSpillModelRuns",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ShorelineImpactGeoJson",
                table: "OilSpillModelRuns",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Scenario",
                table: "OilSpillTrajectoryPoints");

            migrationBuilder.DropColumn(
                name: "HasMitigatedScenario",
                table: "OilSpillModelRuns");

            migrationBuilder.DropColumn(
                name: "MitigatedShorelineImpactGeoJson",
                table: "OilSpillModelRuns");

            migrationBuilder.DropColumn(
                name: "MitigatedShorelineImpactIndex",
                table: "OilSpillModelRuns");

            migrationBuilder.DropColumn(
                name: "MitigatedShorelineImpactTime",
                table: "OilSpillModelRuns");

            migrationBuilder.DropColumn(
                name: "ShorelineImpactGeoJson",
                table: "OilSpillModelRuns");
        }
    }
}

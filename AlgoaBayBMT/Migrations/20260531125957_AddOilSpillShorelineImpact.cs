using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class AddOilSpillShorelineImpact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ShorelineImpactIndex",
                table: "OilSpillModelRuns",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ShorelineImpactTime",
                table: "OilSpillModelRuns",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ShorelineImpactIndex",
                table: "OilSpillModelRuns");

            migrationBuilder.DropColumn(
                name: "ShorelineImpactTime",
                table: "OilSpillModelRuns");
        }
    }
}

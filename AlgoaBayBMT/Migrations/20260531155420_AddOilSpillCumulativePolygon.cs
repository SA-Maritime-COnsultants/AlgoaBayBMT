using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class AddOilSpillCumulativePolygon : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CumulativePolygonGeoJson",
                table: "OilSpillTrajectoryPoints",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CumulativePolygonGeoJson",
                table: "OilSpillTrajectoryPoints");
        }
    }
}

using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class AddResponseActionGeometry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GeometryGeoJson",
                table: "OilSpillResponseActions",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "RadiusMeters",
                table: "OilSpillResponseActions",
                type: "float",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "GeometryGeoJson",
                table: "OilSpillResponseActions");

            migrationBuilder.DropColumn(
                name: "RadiusMeters",
                table: "OilSpillResponseActions");
        }
    }
}

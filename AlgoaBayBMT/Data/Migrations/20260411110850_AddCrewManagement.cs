using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCrewManagement : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "ContractEndDate",
                table: "CrewDeployments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ContractStartDate",
                table: "CrewDeployments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisembarkationPort",
                table: "CrewDeployments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisembarkationReason",
                table: "CrewDeployments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Duties",
                table: "CrewDeployments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmbarkationPort",
                table: "CrewDeployments",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "MedicalFitnessExpiry",
                table: "CrewDeployments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VaccinationStatus",
                table: "CrewDeployments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CrewChangeHistory",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    VesselId = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: true),
                    DeploymentId = table.Column<int>(type: "int", nullable: true),
                    ActionType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ChangedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    ChangedByName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ChangedBySurname = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    ChangedByRank = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ChangedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    BeforeJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    AfterJson = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewChangeHistory", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewChangeHistory_Vessels_VesselId",
                        column: x => x.VesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CrewMemberDetails",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    GivenNames = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    DateOfBirth = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlaceOfBirth = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    Nationality = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PassportNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PassportExpiry = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CrewMemberDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CrewMemberDetails_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CrewChangeHistory_VesselId",
                table: "CrewChangeHistory",
                column: "VesselId");

            migrationBuilder.CreateIndex(
                name: "IX_CrewMemberDetails_UserId",
                table: "CrewMemberDetails",
                column: "UserId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CrewChangeHistory");

            migrationBuilder.DropTable(
                name: "CrewMemberDetails");

            migrationBuilder.DropColumn(
                name: "ContractEndDate",
                table: "CrewDeployments");

            migrationBuilder.DropColumn(
                name: "ContractStartDate",
                table: "CrewDeployments");

            migrationBuilder.DropColumn(
                name: "DisembarkationPort",
                table: "CrewDeployments");

            migrationBuilder.DropColumn(
                name: "DisembarkationReason",
                table: "CrewDeployments");

            migrationBuilder.DropColumn(
                name: "Duties",
                table: "CrewDeployments");

            migrationBuilder.DropColumn(
                name: "EmbarkationPort",
                table: "CrewDeployments");

            migrationBuilder.DropColumn(
                name: "MedicalFitnessExpiry",
                table: "CrewDeployments");

            migrationBuilder.DropColumn(
                name: "VaccinationStatus",
                table: "CrewDeployments");
        }
    }
}

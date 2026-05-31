using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentFormsAndCommander : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Commander",
                table: "OilSpillIncidents",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "IncidentForms",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    IncidentId = table.Column<int>(type: "int", nullable: false),
                    FormType = table.Column<int>(type: "int", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_IncidentForms_IncidentId",
                table: "IncidentForms",
                column: "IncidentId");

            migrationBuilder.CreateIndex(
                name: "IX_IncidentForms_IncidentId_FormType",
                table: "IncidentForms",
                columns: new[] { "IncidentId", "FormType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IncidentForms");

            migrationBuilder.DropColumn(
                name: "Commander",
                table: "OilSpillIncidents");
        }
    }
}

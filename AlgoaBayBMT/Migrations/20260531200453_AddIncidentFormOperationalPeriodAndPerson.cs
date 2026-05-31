using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class AddIncidentFormOperationalPeriodAndPerson : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "OperationalPeriodId",
                table: "IncidentForms",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PersonName",
                table: "IncidentForms",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_IncidentForms_IncidentId_FormType_OperationalPeriodId",
                table: "IncidentForms",
                columns: new[] { "IncidentId", "FormType", "OperationalPeriodId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_IncidentForms_IncidentId_FormType_OperationalPeriodId",
                table: "IncidentForms");

            migrationBuilder.DropColumn(
                name: "OperationalPeriodId",
                table: "IncidentForms");

            migrationBuilder.DropColumn(
                name: "PersonName",
                table: "IncidentForms");
        }
    }
}

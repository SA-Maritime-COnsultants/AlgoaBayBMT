using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixedUserDatabaseChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anchorages_Bays_BayId",
                table: "Anchorages");

            migrationBuilder.DropForeignKey(
                name: "FK_Anchorages_Ports_PortId",
                table: "Anchorages");

            migrationBuilder.DropForeignKey(
                name: "FK_Bays_Ports_PortId",
                table: "Bays");

            migrationBuilder.AddForeignKey(
                name: "FK_Anchorages_Bays_BayId",
                table: "Anchorages",
                column: "BayId",
                principalTable: "Bays",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Anchorages_Ports_PortId",
                table: "Anchorages",
                column: "PortId",
                principalTable: "Ports",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Bays_Ports_PortId",
                table: "Bays",
                column: "PortId",
                principalTable: "Ports",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Anchorages_Bays_BayId",
                table: "Anchorages");

            migrationBuilder.DropForeignKey(
                name: "FK_Anchorages_Ports_PortId",
                table: "Anchorages");

            migrationBuilder.DropForeignKey(
                name: "FK_Bays_Ports_PortId",
                table: "Bays");

            migrationBuilder.AddForeignKey(
                name: "FK_Anchorages_Bays_BayId",
                table: "Anchorages",
                column: "BayId",
                principalTable: "Bays",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Anchorages_Ports_PortId",
                table: "Anchorages",
                column: "PortId",
                principalTable: "Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Bays_Ports_PortId",
                table: "Bays",
                column: "PortId",
                principalTable: "Ports",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}

using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class AddedBunkeringModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<double>(
                name: "LengthOverall",
                table: "Vessels",
                type: "float(18)",
                precision: 18,
                scale: 2,
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float",
                oldNullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Beam",
                table: "Vessels",
                type: "float(18)",
                precision: 18,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LOA",
                table: "Vessels",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsBunkerManager",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "BunkerFuels",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: false),
                    Code = table.Column<string>(type: "nvarchar(40)", maxLength: 40, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkerFuels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ISGOTTStageTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISGOTTStageTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BunkeringOperations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BunkerVesselId = table.Column<int>(type: "int", nullable: false),
                    CustomerVesselId = table.Column<int>(type: "int", nullable: false),
                    BunkerFuelId = table.Column<int>(type: "int", nullable: false),
                    TotalQuantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AlongsideTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CastOffTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletionTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JobStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    JobEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PumpingStopTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkeringOperations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BunkeringOperations_BunkerFuels_BunkerFuelId",
                        column: x => x.BunkerFuelId,
                        principalTable: "BunkerFuels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BunkeringOperations_Vessels_BunkerVesselId",
                        column: x => x.BunkerVesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BunkeringOperations_Vessels_CustomerVesselId",
                        column: x => x.CustomerVesselId,
                        principalTable: "Vessels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ISGOTTItemTemplates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageTemplateId = table.Column<int>(type: "int", nullable: false),
                    Text = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ItemType = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    IsMandatory = table.Column<bool>(type: "bit", nullable: false),
                    Order = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISGOTTItemTemplates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ISGOTTItemTemplates_ISGOTTStageTemplates_StageTemplateId",
                        column: x => x.StageTemplateId,
                        principalTable: "ISGOTTStageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "BunkeringOperationPumpingIntervals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BunkeringOperationId = table.Column<int>(type: "int", nullable: false),
                    PumpStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PumpEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BunkerFuelId = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: false),
                    WindSpeed = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    WindDirection = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: true),
                    SeaState = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Swell = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Visibility = table.Column<string>(type: "nvarchar(120)", maxLength: 120, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BunkeringOperationPumpingIntervals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BunkeringOperationPumpingIntervals_BunkerFuels_BunkerFuelId",
                        column: x => x.BunkerFuelId,
                        principalTable: "BunkerFuels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_BunkeringOperationPumpingIntervals_BunkeringOperations_BunkeringOperationId",
                        column: x => x.BunkeringOperationId,
                        principalTable: "BunkeringOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ISGOTTChecklistStageResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BunkeringOperationId = table.Column<int>(type: "int", nullable: false),
                    StageTemplateId = table.Column<int>(type: "int", nullable: false),
                    CompletedByUserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISGOTTChecklistStageResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ISGOTTChecklistStageResponses_BunkeringOperations_BunkeringOperationId",
                        column: x => x.BunkeringOperationId,
                        principalTable: "BunkeringOperations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ISGOTTChecklistStageResponses_ISGOTTStageTemplates_StageTemplateId",
                        column: x => x.StageTemplateId,
                        principalTable: "ISGOTTStageTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ISGOTTChecklistItemResponses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StageResponseId = table.Column<int>(type: "int", nullable: false),
                    ItemTemplateId = table.Column<int>(type: "int", nullable: false),
                    YesNoNaValue = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    TextValue = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    NumericValue = table.Column<decimal>(type: "decimal(18,3)", precision: 18, scale: 3, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ISGOTTChecklistItemResponses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ISGOTTChecklistItemResponses_ISGOTTChecklistStageResponses_StageResponseId",
                        column: x => x.StageResponseId,
                        principalTable: "ISGOTTChecklistStageResponses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ISGOTTChecklistItemResponses_ISGOTTItemTemplates_ItemTemplateId",
                        column: x => x.ItemTemplateId,
                        principalTable: "ISGOTTItemTemplates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BunkerFuels_Code",
                table: "BunkerFuels",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringOperationPumpingIntervals_BunkerFuelId",
                table: "BunkeringOperationPumpingIntervals",
                column: "BunkerFuelId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringOperationPumpingIntervals_BunkeringOperationId",
                table: "BunkeringOperationPumpingIntervals",
                column: "BunkeringOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringOperations_BunkerFuelId",
                table: "BunkeringOperations",
                column: "BunkerFuelId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringOperations_BunkerVesselId",
                table: "BunkeringOperations",
                column: "BunkerVesselId");

            migrationBuilder.CreateIndex(
                name: "IX_BunkeringOperations_CustomerVesselId",
                table: "BunkeringOperations",
                column: "CustomerVesselId");

            migrationBuilder.CreateIndex(
                name: "IX_ISGOTTChecklistItemResponses_ItemTemplateId",
                table: "ISGOTTChecklistItemResponses",
                column: "ItemTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ISGOTTChecklistItemResponses_StageResponseId",
                table: "ISGOTTChecklistItemResponses",
                column: "StageResponseId");

            migrationBuilder.CreateIndex(
                name: "IX_ISGOTTChecklistStageResponses_BunkeringOperationId",
                table: "ISGOTTChecklistStageResponses",
                column: "BunkeringOperationId");

            migrationBuilder.CreateIndex(
                name: "IX_ISGOTTChecklistStageResponses_StageTemplateId",
                table: "ISGOTTChecklistStageResponses",
                column: "StageTemplateId");

            migrationBuilder.CreateIndex(
                name: "IX_ISGOTTItemTemplates_StageTemplateId",
                table: "ISGOTTItemTemplates",
                column: "StageTemplateId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BunkeringOperationPumpingIntervals");

            migrationBuilder.DropTable(
                name: "ISGOTTChecklistItemResponses");

            migrationBuilder.DropTable(
                name: "ISGOTTChecklistStageResponses");

            migrationBuilder.DropTable(
                name: "ISGOTTItemTemplates");

            migrationBuilder.DropTable(
                name: "BunkeringOperations");

            migrationBuilder.DropTable(
                name: "ISGOTTStageTemplates");

            migrationBuilder.DropTable(
                name: "BunkerFuels");

            migrationBuilder.DropColumn(
                name: "Beam",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "LOA",
                table: "Vessels");

            migrationBuilder.DropColumn(
                name: "IsBunkerManager",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<double>(
                name: "LengthOverall",
                table: "Vessels",
                type: "float",
                nullable: true,
                oldClrType: typeof(double),
                oldType: "float(18)",
                oldPrecision: 18,
                oldScale: 2,
                oldNullable: true);
        }
    }
}

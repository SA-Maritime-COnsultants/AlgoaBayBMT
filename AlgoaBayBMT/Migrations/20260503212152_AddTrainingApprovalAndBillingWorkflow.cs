using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class AddTrainingApprovalAndBillingWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ApprovalNotes",
                table: "UserTrainingAssignments",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "UserTrainingAssignments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOnUtc",
                table: "UserTrainingAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedOnUtc",
                table: "UserTrainingAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompletionScorePercent",
                table: "UserTrainingAssignments",
                type: "decimal(5,2)",
                precision: 5,
                scale: 2,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "UserTrainingAssignments",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaidByUserId",
                table: "UserTrainingAssignments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidOnUtc",
                table: "UserTrainingAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RegisteredByUserId",
                table: "UserTrainingAssignments",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "RegisteredOnUtc",
                table: "UserTrainingAssignments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "Cost",
                table: "Courses",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_ApprovedOnUtc",
                table: "UserTrainingAssignments",
                column: "ApprovedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_InvoiceId",
                table: "UserTrainingAssignments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_Status",
                table: "UserTrainingAssignments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_UserTrainingAssignments_UserId_Status",
                table: "UserTrainingAssignments",
                columns: new[] { "UserId", "Status" });

            migrationBuilder.AddForeignKey(
                name: "FK_UserTrainingAssignments_Invoices_InvoiceId",
                table: "UserTrainingAssignments",
                column: "InvoiceId",
                principalTable: "Invoices",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserTrainingAssignments_Invoices_InvoiceId",
                table: "UserTrainingAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserTrainingAssignments_ApprovedOnUtc",
                table: "UserTrainingAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserTrainingAssignments_InvoiceId",
                table: "UserTrainingAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserTrainingAssignments_Status",
                table: "UserTrainingAssignments");

            migrationBuilder.DropIndex(
                name: "IX_UserTrainingAssignments_UserId_Status",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "ApprovalNotes",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "ApprovedOnUtc",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "CompletedOnUtc",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "CompletionScorePercent",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "PaidByUserId",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "PaidOnUtc",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "RegisteredByUserId",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "RegisteredOnUtc",
                table: "UserTrainingAssignments");

            migrationBuilder.DropColumn(
                name: "Cost",
                table: "Courses");
        }
    }
}

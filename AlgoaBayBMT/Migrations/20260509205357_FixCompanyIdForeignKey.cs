using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlgoaBayBMT.Migrations
{
    /// <inheritdoc />
    public partial class FixCompanyIdForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // The old FK pointed to the wrong table (BunkeringCompanies).
            // That constraint was dropped manually. This migration adds the correct FK.
            migrationBuilder.Sql("""
                IF NOT EXISTS (
                    SELECT 1 FROM sys.foreign_keys
                    WHERE name = 'FK_AspNetUsers_BunkerOperators_CompanyId'
                      AND parent_object_id = OBJECT_ID('AspNetUsers')
                )
                    ALTER TABLE [AspNetUsers]
                    ADD CONSTRAINT [FK_AspNetUsers_BunkerOperators_CompanyId]
                    FOREIGN KEY ([CompanyId]) REFERENCES [BunkerOperators] ([Id])
                    ON DELETE SET NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_BunkerOperators_CompanyId",
                table: "AspNetUsers");
        }
    }
}

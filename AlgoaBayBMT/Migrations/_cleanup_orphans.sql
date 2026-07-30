-- One-time cleanup: drop legacy orphan tables that have no matching C# entities
-- and conflict with the AddCrewingModule migration.
SET XACT_ABORT ON;
BEGIN TRAN;

-- Drop the AspNetUsers.VesselId FK + column (legacy)
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_AspNetUsers_Vessels_VesselId')
    ALTER TABLE [dbo].[AspNetUsers] DROP CONSTRAINT [FK_AspNetUsers_Vessels_VesselId];
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AspNetUsers_VesselId' AND object_id = OBJECT_ID('dbo.AspNetUsers'))
    DROP INDEX [IX_AspNetUsers_VesselId] ON [dbo].[AspNetUsers];
IF COL_LENGTH('dbo.AspNetUsers','VesselId') IS NOT NULL
    ALTER TABLE [dbo].[AspNetUsers] DROP COLUMN [VesselId];

-- Drop dependent tables in order
IF OBJECT_ID('dbo.VesselCrewListEntries','U') IS NOT NULL DROP TABLE [dbo].[VesselCrewListEntries];
IF OBJECT_ID('dbo.VesselRoleAssignments','U') IS NOT NULL DROP TABLE [dbo].[VesselRoleAssignments];
IF OBJECT_ID('dbo.VesselAssignments','U') IS NOT NULL DROP TABLE [dbo].[VesselAssignments];
IF OBJECT_ID('dbo.CrewDeploymentComplianceSnapshots','U') IS NOT NULL DROP TABLE [dbo].[CrewDeploymentComplianceSnapshots];
IF OBJECT_ID('dbo.CrewDeployments','U') IS NOT NULL DROP TABLE [dbo].[CrewDeployments];
IF OBJECT_ID('dbo.CrewChangeHistory','U') IS NOT NULL DROP TABLE [dbo].[CrewChangeHistory];
IF OBJECT_ID('dbo.CrewMemberDetails','U') IS NOT NULL DROP TABLE [dbo].[CrewMemberDetails];
IF OBJECT_ID('dbo.Certifications','U') IS NOT NULL DROP TABLE [dbo].[Certifications];
IF OBJECT_ID('dbo.CrewMembers','U') IS NOT NULL DROP TABLE [dbo].[CrewMembers];
IF OBJECT_ID('dbo.Vessels','U') IS NOT NULL DROP TABLE [dbo].[Vessels];

COMMIT TRAN;

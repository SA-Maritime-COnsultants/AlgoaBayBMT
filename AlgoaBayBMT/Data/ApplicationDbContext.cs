using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Data
{
    public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
    {
        public DbSet<OperationalArea> OperationalAreas => Set<OperationalArea>();
        public DbSet<Port> Ports => Set<Port>();
        public DbSet<Bay> Bays => Set<Bay>();
        public DbSet<Anchorage> Anchorages => Set<Anchorage>();
        public DbSet<BunkeringCompany> BunkeringCompanies => Set<BunkeringCompany>();
        public DbSet<Vessel> Vessels => Set<Vessel>();
        public DbSet<VesselRoleAssignment> VesselRoleAssignments => Set<VesselRoleAssignment>();
        public DbSet<UserAreaAssignment> UserAreaAssignments => Set<UserAreaAssignment>();
        public DbSet<CompanyAreaAssignment> CompanyAreaAssignments => Set<CompanyAreaAssignment>();
        public DbSet<AuthorityContact> AuthorityContacts => Set<AuthorityContact>();
        public DbSet<AuthorityAreaAssignment> AuthorityAreaAssignments => Set<AuthorityAreaAssignment>();
        public DbSet<AreaNotificationDistributionRule> AreaNotificationDistributionRules => Set<AreaNotificationDistributionRule>();
        public DbSet<CrewDeployment> CrewDeployments => Set<CrewDeployment>();
        public DbSet<CrewDeploymentComplianceSnapshot> CrewDeploymentComplianceSnapshots => Set<CrewDeploymentComplianceSnapshot>();
        public DbSet<VesselCrewListEntry> VesselCrewListEntries => Set<VesselCrewListEntry>();
        public DbSet<CrewMemberDetails> CrewMemberDetails => Set<CrewMemberDetails>();
        public DbSet<CrewChangeHistory> CrewChangeHistory => Set<CrewChangeHistory>();

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<ApplicationUser>(entity =>
            {
                entity.Property(x => x.FullName).HasMaxLength(256);
                entity.Property(x => x.CellNo).HasMaxLength(30);
                entity.Property(x => x.Address).HasMaxLength(300);
                entity.Property(x => x.Country).HasMaxLength(100);
                entity.Property(x => x.RequestedRole).HasMaxLength(64);
                entity.Property(x => x.IsCrew).HasDefaultValue(false);
                entity.Property(x => x.CrewRank)
                    .HasConversion(
                        value => value.HasValue ? value.Value.GetDisplayName() : null,
                        value => CrewRankExtensions.ParseDisplayName(value))
                    .HasMaxLength(50);
                entity.Property(x => x.SidNumber).HasMaxLength(20);
                entity.Property(x => x.SidIssuingCountry).HasMaxLength(100);
                entity.Property(x => x.SidIssuingAuthority).HasMaxLength(150);
                entity.Property(x => x.ProfilePictureContentType).HasMaxLength(100);
                entity.Property(x => x.ApprovalNotes).HasMaxLength(1024);
                entity.Property(x => x.ApprovedByUserId).HasMaxLength(450);
                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.PrimaryArea)
                    .WithMany()
                    .HasForeignKey(x => x.PrimaryAreaId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.Vessel)
                    .WithMany()
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<OperationalArea>(entity =>
            {
                entity.HasIndex(x => x.Code).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.Property(x => x.Description).HasMaxLength(500);
            });

            builder.Entity<Port>(entity =>
            {
                entity.HasIndex(x => new { x.OperationalAreaId, x.Code }).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.Ports)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<Bay>(entity =>
            {
                entity.HasIndex(x => new { x.OperationalAreaId, x.Code }).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.Bays)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Port)
                    .WithMany(x => x.Bays)
                    .HasForeignKey(x => x.PortId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<Anchorage>(entity =>
            {
                entity.HasIndex(x => new { x.OperationalAreaId, x.Code }).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Code).HasMaxLength(50).IsRequired();
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.Anchorages)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Port)
                    .WithMany(x => x.Anchorages)
                    .HasForeignKey(x => x.PortId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne(x => x.Bay)
                    .WithMany(x => x.Anchorages)
                    .HasForeignKey(x => x.BayId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            builder.Entity<BunkeringCompany>(entity =>
            {
                entity.HasIndex(x => x.RegistrationNumber).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.RegistrationNumber).HasMaxLength(100).IsRequired();
                entity.Property(x => x.ContactEmail).HasMaxLength(256);
                entity.Property(x => x.ContactPhone).HasMaxLength(50);
            });

            builder.Entity<Vessel>(entity =>
            {
                entity.HasIndex(x => x.ImoNumber).IsUnique();
                entity.Property(x => x.Name).HasMaxLength(200).IsRequired();
                entity.Property(x => x.ImoNumber).HasMaxLength(50).IsRequired();
                entity.Property(x => x.CallSign).HasMaxLength(50);
                entity.Property(x => x.FlagState).HasMaxLength(100);
                entity.HasOne(x => x.Company)
                    .WithMany(x => x.Vessels)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<UserAreaAssignment>(entity =>
            {
                entity.HasIndex(x => new { x.UserId, x.OperationalAreaId }).IsUnique();
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.UserAssignments)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>()
                    .WithMany(x => x.UserAreaAssignments)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CompanyAreaAssignment>(entity =>
            {
                entity.HasIndex(x => new { x.CompanyId, x.OperationalAreaId }).IsUnique();
                entity.HasOne(x => x.Company)
                    .WithMany(x => x.AreaAssignments)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.CompanyAssignments)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AuthorityContact>(entity =>
            {
                entity.Property(x => x.AuthorityRole).HasMaxLength(64).IsRequired();
                entity.Property(x => x.FullName).HasMaxLength(200).IsRequired();
                entity.Property(x => x.Email).HasMaxLength(256).IsRequired();
                entity.Property(x => x.PhoneNumber).HasMaxLength(50);
                entity.Property(x => x.UserId).HasMaxLength(450);
            });

            builder.Entity<AuthorityAreaAssignment>(entity =>
            {
                entity.HasIndex(x => new { x.AuthorityContactId, x.OperationalAreaId }).IsUnique();
                entity.HasOne(x => x.AuthorityContact)
                    .WithMany(x => x.AreaAssignments)
                    .HasForeignKey(x => x.AuthorityContactId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.AuthorityAssignments)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<AreaNotificationDistributionRule>(entity =>
            {
                entity.Property(x => x.NotificationCategory).HasMaxLength(100).IsRequired();
                entity.Property(x => x.RecipientRoleName).HasMaxLength(64).IsRequired();
                entity.HasOne(x => x.OperationalArea)
                    .WithMany(x => x.NotificationDistributionRules)
                    .HasForeignKey(x => x.OperationalAreaId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne(x => x.AuthorityContact)
                    .WithMany(x => x.DistributionRules)
                    .HasForeignKey(x => x.AuthorityContactId)
                    .OnDelete(DeleteBehavior.SetNull);
            });

            builder.Entity<VesselRoleAssignment>(entity =>
            {
                entity.HasIndex(x => new { x.UserId, x.VesselId, x.VesselRoleType, x.IsActive });
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.AssignedByUserId).HasMaxLength(450);
                entity.HasOne(x => x.Vessel)
                    .WithMany(x => x.RoleAssignments)
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.Company)
                    .WithMany(x => x.VesselRoleAssignments)
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.SetNull);
                entity.HasOne<ApplicationUser>()
                    .WithMany(x => x.VesselRoleAssignments)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CrewDeployment>(entity =>
            {
                entity.HasIndex(x => new { x.UserId, x.VesselId, x.Status });
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.OnboardRoleName).HasMaxLength(100).IsRequired();
                entity.Property(x => x.DeployedByUserId).HasMaxLength(450);
                entity.Property(x => x.Notes).HasMaxLength(1000);
                entity.Property(x => x.EmbarkationPort).HasMaxLength(100);
                entity.Property(x => x.DisembarkationPort).HasMaxLength(100);
                entity.Property(x => x.DisembarkationReason).HasMaxLength(200);
                entity.Property(x => x.VaccinationStatus).HasMaxLength(200);
                entity.Property(x => x.Duties).HasMaxLength(200);
                entity.HasOne(x => x.Vessel)
                    .WithMany(x => x.CrewDeployments)
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne<ApplicationUser>()
                    .WithMany(x => x.CrewDeployments)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CrewDeploymentComplianceSnapshot>(entity =>
            {
                entity.Property(x => x.ComplianceItemName).HasMaxLength(200).IsRequired();
                entity.HasOne(x => x.CrewDeployment)
                    .WithMany(x => x.ComplianceSnapshots)
                    .HasForeignKey(x => x.CrewDeploymentId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<VesselCrewListEntry>(entity =>
            {
                entity.HasIndex(x => new { x.VesselId, x.UserId, x.IsCurrent });
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.OnboardRoleName).HasMaxLength(100).IsRequired();
                entity.HasOne(x => x.Vessel)
                    .WithMany(x => x.CrewListEntries)
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.Cascade);
                entity.HasOne(x => x.CrewDeployment)
                    .WithMany(x => x.CrewListEntries)
                    .HasForeignKey(x => x.CrewDeploymentId)
                    .OnDelete(DeleteBehavior.NoAction);
                entity.HasOne<ApplicationUser>()
                    .WithMany(x => x.VesselCrewListEntries)
                    .HasForeignKey(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CrewMemberDetails>(entity =>
            {
                entity.HasIndex(x => x.UserId).IsUnique();
                entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.GivenNames).HasMaxLength(150);
                entity.Property(x => x.Gender).HasMaxLength(20);
                entity.Property(x => x.PlaceOfBirth).HasMaxLength(150);
                entity.Property(x => x.Nationality).HasMaxLength(100);
                entity.Property(x => x.PassportNumber).HasMaxLength(50);
                entity.HasOne<ApplicationUser>()
                    .WithOne(x => x.CrewMemberDetails)
                    .HasForeignKey<CrewMemberDetails>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            builder.Entity<CrewChangeHistory>(entity =>
            {
                entity.Property(x => x.ActionType).HasMaxLength(50).IsRequired();
                entity.Property(x => x.ChangedByUserId).HasMaxLength(450).IsRequired();
                entity.Property(x => x.ChangedByName).HasMaxLength(150).IsRequired();
                entity.Property(x => x.ChangedBySurname).HasMaxLength(150).IsRequired();
                entity.Property(x => x.ChangedByRank).HasMaxLength(100).IsRequired();
                entity.Property(x => x.UserId).HasMaxLength(450);
                entity.Property(x => x.Notes).HasMaxLength(500);
                entity.HasOne<Vessel>()
                    .WithMany()
                    .HasForeignKey(x => x.VesselId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}

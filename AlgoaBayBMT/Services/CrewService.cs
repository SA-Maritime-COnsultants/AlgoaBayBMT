using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using AlgoaBayBMT.Shared.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;

namespace AlgoaBayBMT.Services
{
    public class CrewService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        UserManager<ApplicationUser> userManager,
        IApplicationEmailService emailService,
        ILogger<CrewService> logger) : ICrewService
    {
        public async Task<List<CrewMember>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var query = db.CrewMembers.AsNoTracking().Include(x => x.EmployerOperator).AsQueryable();
            if (!includeInactive) query = query.Where(x => x.IsActive);
            return await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(cancellationToken);
        }

        public async Task<List<CrewMember>> GetByCompanyAsync(int bunkerOperatorId, bool includeInactive = false, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var query = db.CrewMembers.AsNoTracking().Include(x => x.EmployerOperator)
                .Where(x => x.EmployerOperatorId == bunkerOperatorId);
            if (!includeInactive) query = query.Where(x => x.IsActive);
            return await query.OrderBy(x => x.LastName).ThenBy(x => x.FirstName).ToListAsync(cancellationToken);
        }

        public async Task<CrewMember?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.CrewMembers.AsNoTracking()
                .Include(x => x.EmployerOperator)
                .Include(x => x.Documents.Where(d => !d.IsDeleted))
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<CrewMember?> GetByApplicationUserIdAsync(string applicationUserId, CancellationToken cancellationToken = default)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(applicationUserId);
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.CrewMembers.AsNoTracking()
                .FirstOrDefaultAsync(x => x.ApplicationUserId == applicationUserId, cancellationToken);
        }

        public async Task<CrewMember> CreateAsync(CrewMember crewMember, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(crewMember);
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                crewMember.CreatedOnUtc = DateTime.UtcNow;
                crewMember.CreatedByUserId = performedByUserId;
                crewMember.IsDeleted = false;
                db.CrewMembers.Add(crewMember);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("CrewMember {Id} '{Last} {First}' created by {User}", crewMember.Id, crewMember.LastName, crewMember.FirstName, performedByUserId);
                return crewMember;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create crew member '{Last} {First}'", crewMember?.LastName, crewMember?.FirstName);
                throw;
            }
        }

        public async Task<CrewProvisionResult> CreateWithIdentityAsync(
            CrewMember crewMember,
            string? performedByUserId,
            string portalBaseUrl,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(crewMember);

            if (string.IsNullOrWhiteSpace(crewMember.Email))
            {
                return CrewProvisionResult.Failure("Email address is required to provision a crew login account.");
            }

            // ── 1. Resolve or create the Identity user ──────────────────────────
            bool userAlreadyExisted = false;
            string? generatedPassword = null;
            ApplicationUser identityUser;

            var existingUser = await userManager.FindByEmailAsync(crewMember.Email);
            if (existingUser is not null)
            {
                // Link to the existing Identity user — do not create a duplicate.
                userAlreadyExisted = true;
                identityUser = existingUser;
                logger.LogInformation(
                    "Crew provision: existing Identity user {UserId} found for email {Email}; linking.",
                    existingUser.Id, crewMember.Email);
            }
            else
            {
                generatedPassword = GeneratePassword();
                identityUser = new ApplicationUser
                {
                    UserName = crewMember.Email,
                    Email = crewMember.Email,
                    FullName = $"{crewMember.FirstName} {crewMember.LastName}".Trim(),
                    CellNo = crewMember.Phone?.Trim(),
                    IsCrew = true,
                    CrewRank = crewMember.Rank,
                    SidNumber = crewMember.SidNumber?.Trim(),
                    SidIssuingCountry = crewMember.SidIssuingCountry?.Trim(),
                    SidIssuingAuthority = crewMember.SidIssuingAuthority?.Trim(),
                    SidExpiryDate = crewMember.SidExpiryDate,
                    EmailConfirmed = true,
                    IsAccountApproved = true,
                    IsActive = true,
                    ApprovalStatus = ApprovalStatus.Approved,
                    RequestedRole = RoleNames.Crew,
                    ApprovedByUserId = performedByUserId,
                    ApprovedOnUtc = DateTime.UtcNow,
                    RegisteredOnUtc = DateTime.UtcNow
                };

                var createResult = await userManager.CreateAsync(identityUser, generatedPassword);
                if (!createResult.Succeeded)
                {
                    var errors = createResult.Errors.Select(e => e.Description).ToArray();
                    logger.LogWarning(
                        "Crew provision: Identity user creation failed for {Email}: {Errors}",
                        crewMember.Email, string.Join("; ", errors));
                    return CrewProvisionResult.Failure(errors);
                }
            }

            // ── 2. Ensure the CREW role is assigned ─────────────────────────────
            if (!await userManager.IsInRoleAsync(identityUser, RoleNames.Crew))
            {
                var roleResult = await userManager.AddToRoleAsync(identityUser, RoleNames.Crew);
                if (!roleResult.Succeeded)
                {
                    var errors = roleResult.Errors.Select(e => e.Description).ToArray();
                    logger.LogWarning(
                        "Crew provision: failed to assign CREW role to {UserId}: {Errors}",
                        identityUser.Id, string.Join("; ", errors));

                    // Role assignment failure is non-fatal for the crew record itself but we
                    // must report it. Only delete the freshly created user to avoid orphans.
                    if (!userAlreadyExisted)
                    {
                        await userManager.DeleteAsync(identityUser);
                    }
                    return CrewProvisionResult.Failure(errors);
                }
            }

            // ── 3. Persist the crew member record linked to the Identity user ───
            crewMember.ApplicationUserId = identityUser.Id;
            CrewMember saved;
            try
            {
                saved = await CreateAsync(crewMember, performedByUserId, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex,
                    "Crew provision: DB save failed for crew member '{Last} {First}' after Identity user created.",
                    crewMember.LastName, crewMember.FirstName);

                // Attempt to clean up the freshly created Identity user so no orphan is left.
                if (!userAlreadyExisted)
                {
                    await userManager.DeleteAsync(identityUser);
                }
                return CrewProvisionResult.Failure($"Failed to save crew member record: {ex.Message}");
            }

            // ── 4. Send welcome email (non-blocking on failure) ─────────────────
            bool emailSent = false;
            if (!userAlreadyExisted && !string.IsNullOrWhiteSpace(generatedPassword))
            {
                try
                {
                    var emailResult = await emailService.SendCrewWelcomeEmailAsync(
                        identityUser, generatedPassword, portalBaseUrl, cancellationToken);
                    emailSent = emailResult.Succeeded;
                    if (!emailSent)
                    {
                        logger.LogWarning(
                            "Crew provision: welcome email not sent for {Email}: {Errors}",
                            crewMember.Email, string.Join("; ", emailResult.Errors));
                    }
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex,
                        "Crew provision: welcome email threw for {Email}. Login credentials: {Pwd}",
                        crewMember.Email, generatedPassword);
                }
            }

            return CrewProvisionResult.Success(saved, identityUser.Id, userAlreadyExisted, emailSent, generatedPassword);
        }

        /// <summary>
        /// Generates a random password that satisfies ASP.NET Identity default rules:
        /// at least 1 uppercase, 1 lowercase, 1 digit, 1 non-alphanumeric, minimum 8 chars.
        /// </summary>
        private static string GeneratePassword()
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "!@#$%^&*";
            const string all = upper + lower + digits + special;

            Span<char> password = stackalloc char[12];
            // Guarantee at least one of each required character class.
            password[0] = upper[RandomNumberGenerator.GetInt32(upper.Length)];
            password[1] = lower[RandomNumberGenerator.GetInt32(lower.Length)];
            password[2] = digits[RandomNumberGenerator.GetInt32(digits.Length)];
            password[3] = special[RandomNumberGenerator.GetInt32(special.Length)];
            for (int i = 4; i < 12; i++)
            {
                password[i] = all[RandomNumberGenerator.GetInt32(all.Length)];
            }
            // Shuffle using Fisher-Yates via RandomNumberGenerator.
            for (int i = 11; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (password[i], password[j]) = (password[j], password[i]);
            }
            return new string(password);
        }

        public async Task<CrewMember> UpdateAsync(CrewMember crewMember, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(crewMember);
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                var existing = await db.CrewMembers.FirstOrDefaultAsync(x => x.Id == crewMember.Id, cancellationToken)
                               ?? throw new InvalidOperationException($"CrewMember {crewMember.Id} not found.");
                existing.ApplicationUserId = crewMember.ApplicationUserId;
                existing.FirstName = crewMember.FirstName;
                existing.LastName = crewMember.LastName;
                existing.MiddleNames = crewMember.MiddleNames;
                existing.DateOfBirth = crewMember.DateOfBirth;
                existing.Nationality = crewMember.Nationality;
                existing.Gender = crewMember.Gender;
                existing.PassportNumber = crewMember.PassportNumber;
                existing.PassportIssuingCountry = crewMember.PassportIssuingCountry;
                existing.PassportExpiryDate = crewMember.PassportExpiryDate;
                existing.NationalIdNumber = crewMember.NationalIdNumber;
                existing.SidNumber = crewMember.SidNumber;
                existing.SidIssuingCountry = crewMember.SidIssuingCountry;
                existing.SidIssuingAuthority = crewMember.SidIssuingAuthority;
                existing.SidExpiryDate = crewMember.SidExpiryDate;
                existing.Rank = crewMember.Rank;
                existing.PrimaryQualification = crewMember.PrimaryQualification;
                existing.EmployerOperatorId = crewMember.EmployerOperatorId;
                existing.EmployerName = crewMember.EmployerName;
                existing.Email = crewMember.Email;
                existing.Phone = crewMember.Phone;
                existing.EmergencyContactNumber = crewMember.EmergencyContactNumber;
                existing.EmergencyContactName = crewMember.EmergencyContactName;
                existing.PhysicalAddress = crewMember.PhysicalAddress;
                existing.IsActive = crewMember.IsActive;
                existing.ModifiedOnUtc = DateTime.UtcNow;
                existing.ModifiedByUserId = performedByUserId;
                await db.SaveChangesAsync(cancellationToken);
                return existing;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update crew member {Id}", crewMember?.Id);
                throw;
            }
        }

        public async Task SoftDeleteAsync(int id, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                var crew = await db.CrewMembers.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
                if (crew is null) return;
                crew.IsDeleted = true;
                crew.IsActive = false;
                crew.DeletedOnUtc = DateTime.UtcNow;
                crew.DeletedByUserId = performedByUserId;
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("CrewMember {Id} soft-deleted by {User}", id, performedByUserId);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to soft-delete crew member {Id}", id);
                throw;
            }
        }

        public async Task<List<CrewDocument>> GetDocumentsAsync(int crewMemberId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.CrewDocuments.AsNoTracking()
                .Where(x => x.CrewMemberId == crewMemberId && !x.IsDeleted)
                .OrderBy(x => x.DocumentType).ThenBy(x => x.Title)
                .ToListAsync(cancellationToken);
        }

        public async Task<CrewDocument> AddDocumentAsync(CrewDocument document, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(document);
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                document.CreatedOnUtc = DateTime.UtcNow;
                document.CreatedByUserId = performedByUserId;
                document.IsDeleted = false;
                db.CrewDocuments.Add(document);
                await db.SaveChangesAsync(cancellationToken);
                return document;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to add document for crew member {Id}", document?.CrewMemberId);
                throw;
            }
        }

        public async Task<CrewDocument> UpdateDocumentAsync(CrewDocument document, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(document);
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                var existing = await db.CrewDocuments.FirstOrDefaultAsync(x => x.Id == document.Id, cancellationToken)
                               ?? throw new InvalidOperationException($"CrewDocument {document.Id} not found.");
                existing.DocumentType = document.DocumentType;
                existing.Title = document.Title;
                existing.IssuingAuthority = document.IssuingAuthority;
                existing.IssuingCountry = document.IssuingCountry;
                existing.DocumentNumber = document.DocumentNumber;
                existing.IssuedDate = document.IssuedDate;
                existing.ExpiryDate = document.ExpiryDate;
                existing.StorageUrl = document.StorageUrl;
                existing.ContentType = document.ContentType;
                existing.Notes = document.Notes;
                existing.ModifiedOnUtc = DateTime.UtcNow;
                existing.ModifiedByUserId = performedByUserId;
                await db.SaveChangesAsync(cancellationToken);
                return existing;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to update document {Id}", document?.Id);
                throw;
            }
        }

        public async Task SoftDeleteDocumentAsync(int documentId, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
                var doc = await db.CrewDocuments.FirstOrDefaultAsync(x => x.Id == documentId, cancellationToken);
                if (doc is null) return;
                doc.IsDeleted = true;
                doc.DeletedOnUtc = DateTime.UtcNow;
                doc.DeletedByUserId = performedByUserId;
                await db.SaveChangesAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to soft-delete document {Id}", documentId);
                throw;
            }
        }
    }
}

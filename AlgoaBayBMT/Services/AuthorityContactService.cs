using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class AuthorityContactService(ApplicationDbContext dbContext) : IAuthorityContactService
    {
        public Task<List<AuthorityContact>> GetAuthorityContactsAsync(CancellationToken cancellationToken = default) =>
            dbContext.AuthorityContacts.Include(x => x.AreaAssignments).ThenInclude(x => x.OperationalArea)
                .OrderBy(x => x.AuthorityRole).ThenBy(x => x.FullName)
                .ToListAsync(cancellationToken);

        public async Task<OperationResult<AuthorityContact>> CreateAuthorityContactAsync(AuthorityContact contact, CancellationToken cancellationToken = default)
        {
            dbContext.AuthorityContacts.Add(contact);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<AuthorityContact>.Success(contact, "Authority contact created.");
        }

        public async Task<OperationResult<AuthorityContact>> UpdateAuthorityContactAsync(AuthorityContact contact, CancellationToken cancellationToken = default)
        {
            var existing = await dbContext.AuthorityContacts.FindAsync([contact.Id], cancellationToken);
            if (existing is null)
            {
                return OperationResult<AuthorityContact>.Failure("Authority contact not found.");
            }

            existing.AuthorityRole = contact.AuthorityRole;
            existing.FullName = contact.FullName;
            existing.Email = contact.Email;
            existing.PhoneNumber = contact.PhoneNumber;
            existing.UserId = contact.UserId;
            existing.IsActive = contact.IsActive;
            existing.ModifiedOnUtc = DateTime.UtcNow;
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult<AuthorityContact>.Success(existing, "Authority contact updated.");
        }

        public async Task<OperationResult> DeleteAuthorityContactAsync(int contactId, CancellationToken cancellationToken = default)
        {
            var contact = await dbContext.AuthorityContacts.FindAsync([contactId], cancellationToken);
            if (contact is null)
            {
                return OperationResult.Failure("Authority contact not found.");
            }

            dbContext.AuthorityContacts.Remove(contact);
            await dbContext.SaveChangesAsync(cancellationToken);
            return OperationResult.Success("Authority contact deleted.");
        }
    }
}

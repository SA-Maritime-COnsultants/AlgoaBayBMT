using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class NotificationRoutingService(ApplicationDbContext dbContext) : INotificationRoutingService
    {
        public async Task<List<NotificationRecipient>> ResolveRecipientsAsync(int operationalAreaId, string notificationCategory, CancellationToken cancellationToken = default)
        {
            var rules = await dbContext.AreaNotificationDistributionRules
                .Include(x => x.AuthorityContact)
                .Include(x => x.Company)
                .Where(x => x.OperationalAreaId == operationalAreaId && x.IsActive && x.NotificationCategory == notificationCategory)
                .ToListAsync(cancellationToken);

            var recipients = new List<NotificationRecipient>();

            foreach (var rule in rules)
            {
                if (rule.AuthorityContact is not null)
                {
                    recipients.Add(new NotificationRecipient
                    {
                        RoleName = rule.RecipientRoleName,
                        DisplayName = rule.AuthorityContact.FullName,
                        Email = rule.AuthorityContact.Email,
                        AuthorityContactId = rule.AuthorityContactId,
                        UserId = rule.AuthorityContact.UserId
                    });
                }

                if (rule.CompanyId.HasValue)
                {
                    var users = await dbContext.Users
                        .Where(x => x.CompanyId == rule.CompanyId && x.Email != null && x.IsActive && x.RequestedRole == rule.RecipientRoleName)
                        .ToListAsync(cancellationToken);

                    recipients.AddRange(users.Select(user => new NotificationRecipient
                    {
                        RoleName = rule.RecipientRoleName,
                        DisplayName = user.FullName ?? user.Email ?? user.Id,
                        Email = user.Email ?? string.Empty,
                        CompanyId = rule.CompanyId,
                        UserId = user.Id
                    }));
                }
            }

            return recipients
                .Where(x => !string.IsNullOrWhiteSpace(x.Email))
                .GroupBy(x => x.Email, StringComparer.OrdinalIgnoreCase)
                .Select(x => x.First())
                .ToList();
        }
    }
}

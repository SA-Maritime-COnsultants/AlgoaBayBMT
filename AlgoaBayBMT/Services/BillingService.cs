using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Shared.Models;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services
{
    public class BillingService(
        IDbContextFactory<ApplicationDbContext> dbContextFactory,
        ILogger<BillingService> logger) : IBillingService
    {
        public async Task<OperatorBillingAccount?> GetAccountAsync(int bunkerOperatorId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.OperatorBillingAccounts.AsNoTracking()
                .FirstOrDefaultAsync(a => a.BunkerOperatorId == bunkerOperatorId, cancellationToken);
        }

        public async Task<OperatorBillingAccount> UpsertAccountAsync(OperatorBillingAccount account, string? performedByUserId, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(account);
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

            var existing = await db.OperatorBillingAccounts
                .FirstOrDefaultAsync(a => a.BunkerOperatorId == account.BunkerOperatorId, cancellationToken);

            if (existing is null)
            {
                account.CreatedOnUtc = DateTime.UtcNow;
                account.CreatedByUserId = performedByUserId;
                db.OperatorBillingAccounts.Add(account);
                await db.SaveChangesAsync(cancellationToken);
                return account;
            }

            existing.AccountNumber = account.AccountNumber;
            existing.BillingContactName = account.BillingContactName;
            existing.BillingEmail = account.BillingEmail;
            existing.BillingPhone = account.BillingPhone;
            existing.BillingAddress = account.BillingAddress;
            existing.Currency = account.Currency;
            existing.PaymentTermsDays = account.PaymentTermsDays;
            existing.IsActive = account.IsActive;
            existing.ModifiedOnUtc = DateTime.UtcNow;
            existing.ModifiedByUserId = performedByUserId;
            await db.SaveChangesAsync(cancellationToken);
            return existing;
        }

        public async Task<List<Invoice>> GetInvoicesForOperatorAsync(int bunkerOperatorId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.Invoices.AsNoTracking()
                .Include(i => i.LineItems)
                .Where(i => i.BunkerOperatorId == bunkerOperatorId)
                .OrderByDescending(i => i.InvoiceDateUtc)
                .ToListAsync(cancellationToken);
        }

        public async Task<Invoice?> GetInvoiceAsync(int invoiceId, CancellationToken cancellationToken = default)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.Invoices.AsNoTracking()
                .Include(i => i.LineItems)
                .Include(i => i.BunkerOperator)
                .Include(i => i.Vessel)
                .Include(i => i.CrewMember)
                .FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken);
        }

        public async Task<List<Invoice>> GetInvoicesByIdsAsync(IEnumerable<int> invoiceIds, CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(invoiceIds);
            var ids = invoiceIds.Distinct().ToList();
            if (ids.Count == 0)
            {
                return [];
            }

            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await db.Invoices.AsNoTracking()
                .Include(i => i.LineItems)
                .Where(i => ids.Contains(i.Id))
                .ToListAsync(cancellationToken);
        }

        public async Task<Invoice> GenerateRemediationInvoiceAsync(
            int bunkerOperatorId,
            int? crewMemberId,
            int? vesselId,
            IEnumerable<Guid> courseIds,
            decimal unitPrice,
            string? performedByUserId,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(courseIds);
            try
            {
                await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);

                var account = await db.OperatorBillingAccounts.FirstOrDefaultAsync(a => a.BunkerOperatorId == bunkerOperatorId, cancellationToken);
                var courseList = courseIds.Distinct().ToList();
                var courses = await db.Courses.AsNoTracking().Where(c => courseList.Contains(c.CourseId)).ToListAsync(cancellationToken);

                CrewMember? crewMember = null;
                if (crewMemberId.HasValue)
                {
                    crewMember = await db.CrewMembers.AsNoTracking().FirstOrDefaultAsync(c => c.Id == crewMemberId.Value, cancellationToken);
                }
                var studentLabel = crewMember is not null
                    ? $"{crewMember.FirstName} {crewMember.LastName}".Trim() + (!string.IsNullOrWhiteSpace(crewMember.SidNumber) ? $" ({crewMember.SidNumber})" : string.Empty)
                    : null;

                var nowUtc = DateTime.UtcNow;
                var invoice = new Invoice
                {
                    InvoiceNumber = $"INV-{nowUtc:yyyyMMdd}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}",
                    OperatorBillingAccountId = account?.Id,
                    BunkerOperatorId = bunkerOperatorId,
                    CrewMemberId = crewMemberId,
                    VesselId = vesselId,
                    InvoiceDateUtc = nowUtc,
                    DueDateUtc = nowUtc.AddDays(account?.PaymentTermsDays ?? 30),
                    Currency = account?.Currency ?? "ZAR",
                    Status = InvoiceStatus.Draft,
                    CreatedOnUtc = nowUtc,
                    CreatedByUserId = performedByUserId
                };

                decimal subtotal = 0m;
                foreach (var courseId in courseList)
                {
                    var course = courses.FirstOrDefault(c => c.CourseId == courseId);
                    var courseCost = course?.Cost ?? unitPrice;
                    var courseTitle = course is null ? $"Compliance remediation course {courseId}" : $"Compliance remediation: {course.Title}";
                    var description = studentLabel is not null ? $"{courseTitle} — {studentLabel}" : courseTitle;
                    var line = new InvoiceLineItem
                    {
                        Description = description,
                        Quantity = 1m,
                        UnitPrice = courseCost,
                        LineTotal = courseCost,
                        RelatedCourseId = courseId
                    };
                    invoice.LineItems.Add(line);
                    subtotal += courseCost;
                }

                invoice.SubTotal = subtotal;
                invoice.TaxAmount = 0m;
                invoice.TotalAmount = subtotal;

                db.Invoices.Add(invoice);
                await db.SaveChangesAsync(cancellationToken);
                logger.LogInformation("Remediation invoice {Number} created for operator {Op} (crew {Crew}, vessel {Vessel})",
                    invoice.InvoiceNumber, bunkerOperatorId, crewMemberId, vesselId);
                return invoice;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to generate remediation invoice for operator {Op}", bunkerOperatorId);
                throw;
            }
        }

        public async Task<Invoice> IssueAsync(int invoiceId, string? performedByUserId, CancellationToken cancellationToken = default)
            => await SetStatusAsync(invoiceId, InvoiceStatus.Issued, performedByUserId, cancellationToken);

        public async Task<Invoice> MarkPaidAsync(int invoiceId, string? performedByUserId, CancellationToken cancellationToken = default)
            => await SetStatusAsync(invoiceId, InvoiceStatus.Paid, performedByUserId, cancellationToken);

        public async Task<Invoice> CancelAsync(int invoiceId, string? performedByUserId, CancellationToken cancellationToken = default)
            => await SetStatusAsync(invoiceId, InvoiceStatus.Cancelled, performedByUserId, cancellationToken);

        private async Task<Invoice> SetStatusAsync(int invoiceId, InvoiceStatus status, string? performedByUserId, CancellationToken cancellationToken)
        {
            await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            var invoice = await db.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId, cancellationToken)
                          ?? throw new InvalidOperationException($"Invoice {invoiceId} not found.");
            invoice.Status = status;
            invoice.ModifiedOnUtc = DateTime.UtcNow;
            invoice.ModifiedByUserId = performedByUserId;
            await db.SaveChangesAsync(cancellationToken);
            return invoice;
        }
    }
}

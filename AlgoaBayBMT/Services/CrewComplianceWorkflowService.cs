using AlgoaBayBMT.Data;
using AlgoaBayBMT.Services.Interfaces;
using AlgoaBayBMT.Services.Models;
using AlgoaBayBMT.Shared.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace AlgoaBayBMT.Services;

public sealed class CrewComplianceWorkflowService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    UserManager<ApplicationUser> userManager,
    ICrewService crewService,
    IBillingService billingService,
    IApplicationEmailService emailService,
    INotificationService notificationService,
    ILogger<CrewComplianceWorkflowService> logger) : ICrewComplianceWorkflowService
{
    public async Task<CrewComplianceDashboardModel?> GetCrewDashboardAsync(int crewMemberId, int? vesselId = null, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var crew = await dbContext.CrewMembers.AsNoTracking()
            .Include(x => x.EmployerOperator)
            .FirstOrDefaultAsync(x => x.Id == crewMemberId, cancellationToken);
        if (crew is null)
        {
            return null;
        }

        var snapshot = await BuildCrewDashboardAsync(dbContext, crew, vesselId, cancellationToken);
        return snapshot;
    }

    public async Task<List<CrewComplianceDashboardModel>> GetCrewRegisterDashboardAsync(CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var crews = await dbContext.CrewMembers.AsNoTracking()
            .Include(x => x.EmployerOperator)
            .OrderBy(x => x.LastName)
            .ThenBy(x => x.FirstName)
            .ToListAsync(cancellationToken);

        var results = new List<CrewComplianceDashboardModel>(crews.Count);
        foreach (var crew in crews)
        {
            var dashboard = await BuildCrewDashboardAsync(dbContext, crew, vesselId: null, cancellationToken);
            results.Add(dashboard);
        }

        return results;
    }

    public async Task<List<TrainingApprovalQueueItemModel>> GetPendingApprovalsAsync(int? operatorId = null, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var query = dbContext.UserTrainingAssignments.AsNoTracking()
            .Where(x => x.Status == AssignmentStatus.PendingApproval)
            .Join(dbContext.CrewMembers.AsNoTracking(),
                assignment => assignment.UserId,
                crew => crew.ApplicationUserId!,
                (assignment, crew) => new { assignment, crew })
            .Join(dbContext.Courses.AsNoTracking(),
                joined => joined.assignment.CourseId,
                course => course.CourseId,
                (joined, course) => new { joined.assignment, joined.crew, course });

        if (operatorId.HasValue)
        {
            query = query.Where(x => x.crew.EmployerOperatorId == operatorId.Value);
        }

        return await query
            .OrderBy(x => x.assignment.RegisteredOnUtc ?? x.assignment.AssignedOnUtc)
            .Select(x => new TrainingApprovalQueueItemModel
            {
                AssignmentId = x.assignment.UserTrainingAssignmentId,
                CrewMemberId = x.crew.Id,
                CrewMemberName = (x.crew.FirstName + " " + x.crew.LastName).Trim(),
                OperatorId = x.crew.EmployerOperatorId,
                OperatorName = x.crew.EmployerOperator != null ? x.crew.EmployerOperator.Name : x.crew.EmployerName,
                CrewEmail = x.crew.Email,
                CourseId = x.course.CourseId,
                CourseCode = x.course.Code,
                CourseTitle = x.course.Title,
                CourseCost = x.course.Cost,
                RegisteredOnUtc = x.assignment.RegisteredOnUtc ?? x.assignment.AssignedOnUtc,
                RegisteredByUserId = x.assignment.RegisteredByUserId,
                Reason = x.assignment.Reason
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<OperationResult<TrainingRegistrationSubmissionResultModel>> SubmitTrainingRegistrationAsync(TrainingRegistrationSelectionModel request, string? performedByUserId, CancellationToken cancellationToken = default)
    {
        if (request.CourseIds.Count == 0)
        {
            return OperationResult<TrainingRegistrationSubmissionResultModel>.Failure("Select at least one course.");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var crew = await dbContext.CrewMembers.FirstOrDefaultAsync(x => x.Id == request.CrewMemberId, cancellationToken);
        if (crew is null)
        {
            return OperationResult<TrainingRegistrationSubmissionResultModel>.Failure("Crew member not found.");
        }

        if (string.IsNullOrWhiteSpace(crew.ApplicationUserId))
        {
            return OperationResult<TrainingRegistrationSubmissionResultModel>.Failure("Crew member must have a linked login account before training registration can be submitted.");
        }

        var distinctCourseIds = request.CourseIds.Distinct().ToList();
        var validCourseIds = await dbContext.Courses.AsNoTracking()
            .Where(x => distinctCourseIds.Contains(x.CourseId) && x.IsActive)
            .Select(x => x.CourseId)
            .ToListAsync(cancellationToken);

        if (validCourseIds.Count == 0)
        {
            return OperationResult<TrainingRegistrationSubmissionResultModel>.Failure("No valid training courses were selected.");
        }

        var existingAssignments = await dbContext.UserTrainingAssignments
            .Where(x => x.UserId == crew.ApplicationUserId && validCourseIds.Contains(x.CourseId))
            .ToListAsync(cancellationToken);

        var createdIds = new List<Guid>();
        foreach (var courseId in validCourseIds)
        {
            var existing = existingAssignments.FirstOrDefault(x => x.CourseId == courseId);
            if (existing is not null && existing.Status is not AssignmentStatus.Cancelled and not AssignmentStatus.Expired)
            {
                continue;
            }

            if (existing is not null)
            {
                existing.Status = AssignmentStatus.PendingApproval;
                existing.RegisteredByUserId = performedByUserId;
                existing.RegisteredOnUtc = DateTime.UtcNow;
                existing.DueDateUtc = request.DueDateUtc;
                existing.Reason = request.Reason;
                existing.ApprovedByUserId = null;
                existing.ApprovedOnUtc = null;
                existing.ApprovalNotes = null;
                existing.InvoiceId = null;
                existing.CompletedOnUtc = null;
                existing.CompletionScorePercent = null;
                existing.PaidOnUtc = null;
                existing.PaidByUserId = null;
                createdIds.Add(existing.UserTrainingAssignmentId);
                continue;
            }

            var assignment = new UserTrainingAssignment
            {
                UserTrainingAssignmentId = Guid.NewGuid(),
                UserId = crew.ApplicationUserId,
                CourseId = courseId,
                AssignedByUserId = performedByUserId,
                AssignedOnUtc = DateTime.UtcNow,
                RegisteredByUserId = performedByUserId,
                RegisteredOnUtc = DateTime.UtcNow,
                DueDateUtc = request.DueDateUtc,
                Reason = request.Reason,
                Status = AssignmentStatus.PendingApproval
            };
            dbContext.UserTrainingAssignments.Add(assignment);
            createdIds.Add(assignment.UserTrainingAssignmentId);
        }

        if (createdIds.Count == 0)
        {
            return OperationResult<TrainingRegistrationSubmissionResultModel>.Failure("No new registrations were submitted because matching active registrations already exist.");
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        logger.LogInformation("Submitted {Count} training registrations for crew {CrewMemberId}", createdIds.Count, request.CrewMemberId);

        return OperationResult<TrainingRegistrationSubmissionResultModel>.Success(
            new TrainingRegistrationSubmissionResultModel
            {
                CreatedCount = createdIds.Count,
                AssignmentIds = createdIds
            },
            "Training registration submitted for approval.");
    }

    public async Task<OperationResult<TrainingApprovalResultModel>> ApproveRegistrationsAsync(IEnumerable<Guid> assignmentIds, string? performedByUserId, string portalBaseUrl, CancellationToken cancellationToken = default)
    {
        var assignmentIdList = assignmentIds.Distinct().ToList();
        if (assignmentIdList.Count == 0)
        {
            return OperationResult<TrainingApprovalResultModel>.Failure("Select at least one registration to approve.");
        }

        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var assignments = await dbContext.UserTrainingAssignments
            .Include(x => x.Course)
            .Where(x => assignmentIdList.Contains(x.UserTrainingAssignmentId))
            .ToListAsync(cancellationToken);

        if (assignments.Count == 0)
        {
            return OperationResult<TrainingApprovalResultModel>.Failure("No matching registrations were found.");
        }

        var users = await dbContext.Users
            .Where(x => assignments.Select(a => a.UserId).Contains(x.Id))
            .ToListAsync(cancellationToken);
        var crews = await dbContext.CrewMembers
            .Include(x => x.EmployerOperator)
            .Where(x => assignments.Select(a => a.UserId).Contains(x.ApplicationUserId!))
            .ToListAsync(cancellationToken);

        var result = new TrainingApprovalResultModel();
        var invoiceIds = new HashSet<int>();
        var nowUtc = DateTime.UtcNow;

        var groupedAssignments = assignments
            .Where(x => x.Status == AssignmentStatus.PendingApproval)
            .GroupBy(x => x.UserId, StringComparer.OrdinalIgnoreCase)
            .ToList();

        foreach (var group in groupedAssignments)
        {
            var crew = crews.FirstOrDefault(x => string.Equals(x.ApplicationUserId, group.Key, StringComparison.OrdinalIgnoreCase));
            if (crew?.EmployerOperatorId is null)
            {
                continue;
            }

            var approvedAssignments = group.ToList();
            var courseIds = approvedAssignments.Select(x => x.CourseId).Distinct().ToList();
            var totalCost = approvedAssignments.Sum(x => x.Course?.Cost ?? 0m);
            var unitPrice = approvedAssignments.Count == 0 ? 0m : approvedAssignments.Average(x => x.Course?.Cost ?? 0m);
            if (totalCost <= 0m)
            {
                unitPrice = approvedAssignments.FirstOrDefault()?.Course?.Cost ?? 0m;
            }

            var invoice = await billingService.GenerateRemediationInvoiceAsync(
                crew.EmployerOperatorId.Value,
                crew.Id,
                null,
                courseIds,
                unitPrice,
                performedByUserId,
                cancellationToken);

            invoiceIds.Add(invoice.Id);

            foreach (var assignment in approvedAssignments)
            {
                assignment.Status = AssignmentStatus.Assigned;
                assignment.ApprovedByUserId = performedByUserId;
                assignment.ApprovedOnUtc = nowUtc;
                assignment.InvoiceId = invoice.Id;
                result.AssignmentIds.Add(assignment.UserTrainingAssignmentId);
            }

            result.ApprovedCount += approvedAssignments.Count;

            var user = users.FirstOrDefault(x => string.Equals(x.Id, group.Key, StringComparison.OrdinalIgnoreCase));
            if (user is not null)
            {
                result.CredentialsEnsured = true;
                if (!string.IsNullOrWhiteSpace(user.Email))
                {
                    var emailResult = await emailService.SendWelcomeEmailWithResultAsync(user, portalBaseUrl, cancellationToken);
                    result.EmailSent = result.EmailSent || emailResult.Succeeded;
                }

                await notificationService.QueueAsync(
                    recipientUserId: user.Id,
                    recipientEmail: user.Email,
                    recipientDisplayName: user.FullName,
                    subject: "Training registration approved",
                    body: $"Your training registration has been approved and invoice {invoice.InvoiceNumber} was generated for processing.",
                    category: NotificationCategory.TrainingAssignment,
                    channel: string.IsNullOrWhiteSpace(user.Email) ? NotificationChannel.InApp : NotificationChannel.Both,
                    relatedCrewMemberId: crew.Id,
                    relatedInvoiceId: invoice.Id,
                    cancellationToken: cancellationToken);
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        result.InvoiceIds = invoiceIds.ToList();

        if (result.ApprovedCount == 0)
        {
            return OperationResult<TrainingApprovalResultModel>.Failure("No pending registrations were approved.");
        }

        logger.LogInformation("Approved {Count} training registrations", result.ApprovedCount);
        return OperationResult<TrainingApprovalResultModel>.Success(result, "Training registrations approved successfully.");
    }

    public async Task<OperationResult> SynchronizeCompletedAssignmentsAsync(string userId, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(userId);
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        var assignments = await dbContext.UserTrainingAssignments
            .Where(x => x.UserId == userId
                && (x.Status == AssignmentStatus.Assigned
                    || x.Status == AssignmentStatus.Started
                    || x.Status == AssignmentStatus.CompletedPendingPayment))
            .ToListAsync(cancellationToken);

        if (assignments.Count == 0)
        {
            return OperationResult.Success();
        }

        var courseIds = assignments.Select(x => x.CourseId).Distinct().ToList();
        var progressRecords = await dbContext.UserCourseProgress.AsNoTracking()
            .Where(x => x.UserId == userId && courseIds.Contains(x.CourseId))
            .ToListAsync(cancellationToken);
        var completionRecords = await dbContext.CourseCompletionRecords.AsNoTracking()
            .Where(x => x.UserId == userId && courseIds.Contains(x.CourseId))
            .ToListAsync(cancellationToken);

        var invoiceIds = assignments.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value).Distinct().ToList();
        var invoiceStatuses = invoiceIds.Count == 0
            ? new Dictionary<int, InvoiceStatus>()
            : await dbContext.Invoices.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, x => x.Status, cancellationToken);

        var updated = 0;
        foreach (var assignment in assignments)
        {
            var completion = completionRecords
                .Where(x => x.CourseId == assignment.CourseId)
                .OrderByDescending(x => x.CompletedOnUtc)
                .FirstOrDefault();
            var progress = progressRecords.FirstOrDefault(x => x.CourseId == assignment.CourseId);

            if (progress?.Status == ProgressStatus.Started && assignment.Status == AssignmentStatus.Assigned)
            {
                assignment.Status = AssignmentStatus.Started;
                updated++;
            }

            if (completion is null)
            {
                continue;
            }

            assignment.CompletedOnUtc ??= completion.CompletedOnUtc;
            assignment.CompletionScorePercent ??= completion.FinalScorePercent;

            var invoicePaid = assignment.InvoiceId.HasValue
                && invoiceStatuses.TryGetValue(assignment.InvoiceId.Value, out var invoiceStatus)
                && invoiceStatus == InvoiceStatus.Paid;

            if (invoicePaid)
            {
                assignment.Status = AssignmentStatus.Completed;
                assignment.PaidOnUtc ??= DateTime.UtcNow;
            }
            else
            {
                assignment.Status = AssignmentStatus.CompletedPendingPayment;
            }

            updated++;
        }

        if (updated > 0)
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return OperationResult.Success();
    }

    public async Task<OperationResult> SynchronizeInvoicePaymentAsync(int invoiceId, string? performedByUserId, CancellationToken cancellationToken = default)
    {
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var invoice = await dbContext.Invoices.AsNoTracking().FirstOrDefaultAsync(x => x.Id == invoiceId, cancellationToken);
        if (invoice is null)
        {
            return OperationResult.Failure("Invoice not found.");
        }

        var assignments = await dbContext.UserTrainingAssignments
            .Where(x => x.InvoiceId == invoiceId)
            .ToListAsync(cancellationToken);

        if (assignments.Count == 0)
        {
            return OperationResult.Success();
        }

        if (invoice.Status == InvoiceStatus.Paid)
        {
            foreach (var assignment in assignments)
            {
                if (assignment.CompletedOnUtc.HasValue || assignment.Status == AssignmentStatus.CompletedPendingPayment)
                {
                    assignment.Status = AssignmentStatus.Completed;
                    assignment.PaidOnUtc = DateTime.UtcNow;
                    assignment.PaidByUserId = performedByUserId;
                }
            }

            await dbContext.SaveChangesAsync(cancellationToken);
        }

        return OperationResult.Success();
    }

    private async Task<CrewComplianceDashboardModel> BuildCrewDashboardAsync(ApplicationDbContext dbContext, CrewMember crew, int? vesselId, CancellationToken cancellationToken)
    {
        var courseQuery = dbContext.Courses.AsNoTracking().Where(x => x.IsActive && x.CurrentVersionId.HasValue);
        var courses = await courseQuery.Select(x => new CourseProjection
        {
            CourseId = x.CourseId,
            Code = x.Code,
            Title = x.Title,
            Summary = x.Summary,
            AudienceSummary = x.TargetAudienceSummary,
            PassMarkPercent = x.PassMarkPercent,
            IsMandatory = x.IsMandatory,
            Cost = x.Cost,
            ValidityMonths = x.ValidityMonths
        }).ToListAsync(cancellationToken);

        var rules = await dbContext.CourseAudienceRules.AsNoTracking()
            .Where(x => courses.Select(c => c.CourseId).Contains(x.CourseId))
            .ToListAsync(cancellationToken);

        var assignments = string.IsNullOrWhiteSpace(crew.ApplicationUserId)
            ? []
            : await dbContext.UserTrainingAssignments.AsNoTracking()
                .Where(x => x.UserId == crew.ApplicationUserId)
                .ToListAsync(cancellationToken);

        var progressRecords = string.IsNullOrWhiteSpace(crew.ApplicationUserId)
            ? []
            : await dbContext.UserCourseProgress.AsNoTracking()
                .Where(x => x.UserId == crew.ApplicationUserId)
                .ToListAsync(cancellationToken);

        var completions = string.IsNullOrWhiteSpace(crew.ApplicationUserId)
            ? []
            : await dbContext.CourseCompletionRecords.AsNoTracking()
                .Where(x => x.UserId == crew.ApplicationUserId)
                .ToListAsync(cancellationToken);

        var invoiceIds = assignments.Where(x => x.InvoiceId.HasValue).Select(x => x.InvoiceId!.Value).Distinct().ToList();
        var invoices = invoiceIds.Count == 0
            ? new Dictionary<int, Invoice>()
            : await dbContext.Invoices.AsNoTracking()
                .Where(x => invoiceIds.Contains(x.Id))
                .ToDictionaryAsync(x => x.Id, cancellationToken);

        var applicableCourses = courses
            .Where(course => IsCourseApplicable(course.CourseId, rules.Where(x => x.CourseId == course.CourseId).ToList(), crew, assignments.Any(a => a.CourseId == course.CourseId)))
            .ToList();

        var items = new List<CrewComplianceCourseItemModel>();
        foreach (var course in applicableCourses)
        {
            var assignment = assignments.Where(x => x.CourseId == course.CourseId).OrderByDescending(x => x.AssignedOnUtc).FirstOrDefault();
            var progress = progressRecords.FirstOrDefault(x => x.CourseId == course.CourseId);
            var completion = completions.Where(x => x.CourseId == course.CourseId).OrderByDescending(x => x.CompletedOnUtc).FirstOrDefault();
            var invoice = assignment?.InvoiceId is int invoiceId && invoices.TryGetValue(invoiceId, out var resolvedInvoice)
                ? resolvedInvoice
                : null;
            var effectiveAssignmentStatus = ResolveEffectiveAssignmentStatus(assignment, completion, invoice);

            var expiresOn = completion?.ExpiryDateUtc;
            var isExpired = expiresOn.HasValue && expiresOn.Value <= DateTime.UtcNow;
            var statusText = BuildStatusText(effectiveAssignmentStatus, assignment, progress, completion, invoice, isExpired);

            items.Add(new CrewComplianceCourseItemModel
            {
                CourseId = course.CourseId,
                AssignmentId = assignment?.UserTrainingAssignmentId,
                Code = course.Code,
                Title = course.Title,
                AudienceSummary = course.AudienceSummary,
                Cost = course.Cost,
                PassMarkPercent = course.PassMarkPercent,
                ProgressPercent = completion is not null && !isExpired ? 100m : progress?.PercentComplete ?? 0m,
                AssignmentStatus = effectiveAssignmentStatus,
                ProgressStatus = progress?.Status,
                DueDateUtc = assignment?.DueDateUtc,
                CompletedOnUtc = completion?.CompletedOnUtc,
                ExpiresOnUtc = expiresOn,
                InvoiceId = invoice?.Id,
                InvoiceStatus = invoice?.Status,
                CompletionScorePercent = assignment?.CompletionScorePercent ?? completion?.FinalScorePercent,
                IsMandatory = course.IsMandatory,
                IsExpired = isExpired,
                CanRegister = assignment is null || effectiveAssignmentStatus is AssignmentStatus.Cancelled or AssignmentStatus.Expired,
                StatusText = statusText
            });
        }

        var requiredCount = items.Count;
        var completedCount = items.Count(x => !x.IsExpired && x.AssignmentStatus == AssignmentStatus.Completed);
        var pendingApprovalCount = items.Count(x => x.AssignmentStatus == AssignmentStatus.PendingApproval);
        var pendingPaymentCount = items.Count(x => x.AssignmentStatus == AssignmentStatus.CompletedPendingPayment);
        var expiredCount = items.Count(x => x.IsExpired || x.AssignmentStatus == AssignmentStatus.Expired);

        var overallStatus = DetermineOverallStatus(items);
        var compliancePercent = requiredCount == 0 ? 100m : Math.Round((decimal)completedCount / requiredCount * 100m, 2);
        var vesselIsNonCompliant = vesselId.HasValue && items.Any(x => x.IsMandatory && x.AssignmentStatus != AssignmentStatus.Completed);

        return new CrewComplianceDashboardModel
        {
            CrewMemberId = crew.Id,
            CrewMemberName = $"{crew.FirstName} {crew.LastName}".Trim(),
            EmployerName = crew.EmployerOperator?.Name ?? crew.EmployerName,
            OverallStatus = overallStatus,
            OverallStatusText = overallStatus switch
            {
                CrewComplianceOverallStatus.FullyCompliant => "Fully Compliant",
                CrewComplianceOverallStatus.PendingTraining => "Pending Training",
                CrewComplianceOverallStatus.PendingApproval => "Pending Approval",
                CrewComplianceOverallStatus.PendingPayment => "Pending Payment",
                CrewComplianceOverallStatus.Expired => "Expired",
                _ => "Unknown"
            },
            Summary = BuildSummary(items),
            RequiredCourseCount = requiredCount,
            CompletedCourseCount = completedCount,
            PendingApprovalCount = pendingApprovalCount,
            PendingPaymentCount = pendingPaymentCount,
            ExpiredCourseCount = expiredCount,
            CompliancePercent = compliancePercent,
            VesselIsNonCompliant = vesselIsNonCompliant,
            Courses = items.OrderByDescending(x => x.IsExpired).ThenBy(x => x.Title).ToList()
        };
    }

    private static CrewComplianceOverallStatus DetermineOverallStatus(List<CrewComplianceCourseItemModel> items)
    {
        if (items.Count == 0)
        {
            return CrewComplianceOverallStatus.FullyCompliant;
        }

        if (items.Any(x => x.IsExpired))
        {
            return CrewComplianceOverallStatus.Expired;
        }

        if (items.Any(x => x.AssignmentStatus == AssignmentStatus.CompletedPendingPayment))
        {
            return CrewComplianceOverallStatus.PendingPayment;
        }

        if (items.Any(x => x.AssignmentStatus == AssignmentStatus.PendingApproval))
        {
            return CrewComplianceOverallStatus.PendingApproval;
        }

        if (items.Any(x => x.AssignmentStatus is null or AssignmentStatus.Assigned or AssignmentStatus.Started))
        {
            return CrewComplianceOverallStatus.PendingTraining;
        }

        return CrewComplianceOverallStatus.FullyCompliant;
    }

    private static string BuildSummary(List<CrewComplianceCourseItemModel> items)
    {
        if (items.Count == 0)
        {
            return "No mandatory training courses are currently required for this crew member.";
        }

        if (items.Any(x => x.IsExpired))
        {
            return "One or more mandatory training certificates have expired and require renewal.";
        }

        if (items.Any(x => x.AssignmentStatus == AssignmentStatus.CompletedPendingPayment))
        {
            return "Mandatory training has been completed, but payment is still outstanding.";
        }

        if (items.Any(x => x.AssignmentStatus == AssignmentStatus.PendingApproval))
        {
            return "Mandatory training registration is awaiting company approval.";
        }

        if (items.Any(x => x.AssignmentStatus is null or AssignmentStatus.Assigned or AssignmentStatus.Started))
        {
            return "Mandatory training is still in progress or has not yet started.";
        }

        return "All mandatory training requirements are currently satisfied.";
    }

    private static AssignmentStatus? ResolveEffectiveAssignmentStatus(UserTrainingAssignment? assignment, CourseCompletionRecord? completion, Invoice? invoice)
    {
        if (assignment is null)
        {
            return completion is not null ? AssignmentStatus.Completed : null;
        }

        if (assignment.Status == AssignmentStatus.CompletedPendingPayment && invoice?.Status == InvoiceStatus.Paid)
        {
            return AssignmentStatus.Completed;
        }

        if (assignment.Status == AssignmentStatus.Assigned && completion is not null)
        {
            return invoice?.Status == InvoiceStatus.Paid
                ? AssignmentStatus.Completed
                : AssignmentStatus.CompletedPendingPayment;
        }

        return assignment.Status;
    }

    private static string BuildStatusText(AssignmentStatus? effectiveStatus, UserTrainingAssignment? assignment, UserCourseProgress? progress, CourseCompletionRecord? completion, Invoice? invoice, bool isExpired)
    {
        if (isExpired)
        {
            return "Expired";
        }
        if (effectiveStatus == AssignmentStatus.PendingApproval)
        {
            return "Pending Approval";
        }
        if (effectiveStatus == AssignmentStatus.CompletedPendingPayment)
        {
            return invoice?.Status == InvoiceStatus.Paid ? "Completed" : "Pending Payment";
        }
        if (effectiveStatus == AssignmentStatus.Completed || (completion is not null && !isExpired))
        {
            return "Completed";
        }
        if (progress?.Status == ProgressStatus.Started || effectiveStatus == AssignmentStatus.Started)
        {
            return "In Progress";
        }
        if (assignment is not null)
        {
            return "Registered";
        }
        return "Not Started";
    }

    private static bool IsCourseApplicable(Guid courseId, List<CourseAudienceRule> rules, CrewMember crew, bool hasAssignment)
    {
        if (hasAssignment)
        {
            return true;
        }

        if (rules.Count == 0)
        {
            return false;
        }

        foreach (var rule in rules)
        {
            switch (rule.RuleType)
            {
                case CourseAudienceRuleType.AllCrew:
                    return true;
                case CourseAudienceRuleType.ApplicationRole:
                    if (string.Equals(rule.ApplicationRoleName, "CREW", StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    break;
                case CourseAudienceRuleType.OnBoardRole:
                    if (!string.IsNullOrWhiteSpace(rule.OnBoardRole)
                        && crew.Rank.HasValue
                        && string.Equals(rule.OnBoardRole, crew.Rank.Value.GetDisplayName(), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    break;
                case CourseAudienceRuleType.Qualification:
                    if (!string.IsNullOrWhiteSpace(rule.Qualification)
                        && crew.PrimaryQualification.HasValue
                        && string.Equals(rule.Qualification, crew.PrimaryQualification.Value.ToString(), StringComparison.OrdinalIgnoreCase))
                    {
                        return true;
                    }
                    break;
                case CourseAudienceRuleType.ManualAssignment:
                    if (hasAssignment)
                    {
                        return true;
                    }
                    break;
            }
        }

        return false;
    }

    private sealed class CourseProjection
    {
        public Guid CourseId { get; set; }
        public string Code { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string? Summary { get; set; }
        public string? AudienceSummary { get; set; }
        public decimal PassMarkPercent { get; set; }
        public bool IsMandatory { get; set; }
        public decimal Cost { get; set; }
        public int ValidityMonths { get; set; }
    }
}

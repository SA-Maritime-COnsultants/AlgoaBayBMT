using AlgoaBayBMT.Shared.Models;

namespace AlgoaBayBMT.Services.Interfaces
{
    public interface ICrewService
    {
        Task<List<CrewMember>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<List<CrewMember>> GetByCompanyAsync(int bunkerOperatorId, bool includeInactive = false, CancellationToken cancellationToken = default);
        Task<CrewMember?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
        Task<CrewMember?> GetByApplicationUserIdAsync(string applicationUserId, CancellationToken cancellationToken = default);
        Task<CrewMember> CreateAsync(CrewMember crewMember, string? performedByUserId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates the crew member record AND provisions a linked Identity user with the CREW role.
        /// Safe to call from an Admin, Captain, or Bunker Manager context.
        /// If an Identity user with the same email already exists, it is linked instead of re-created.
        /// </summary>
        Task<CrewProvisionResult> CreateWithIdentityAsync(CrewMember crewMember, string? performedByUserId, string portalBaseUrl, CancellationToken cancellationToken = default);

        Task<CrewMember> UpdateAsync(CrewMember crewMember, string? performedByUserId, CancellationToken cancellationToken = default);
        Task SoftDeleteAsync(int id, string? performedByUserId, CancellationToken cancellationToken = default);

        Task<List<CrewDocument>> GetDocumentsAsync(int crewMemberId, CancellationToken cancellationToken = default);
        Task<CrewDocument> AddDocumentAsync(CrewDocument document, string? performedByUserId, CancellationToken cancellationToken = default);
        Task<CrewDocument> UpdateDocumentAsync(CrewDocument document, string? performedByUserId, CancellationToken cancellationToken = default);
        Task SoftDeleteDocumentAsync(int documentId, string? performedByUserId, CancellationToken cancellationToken = default);
    }

    /// <summary>Result returned by <see cref="ICrewService.CreateWithIdentityAsync"/>.</summary>
    public sealed class CrewProvisionResult
    {
        public bool Succeeded { get; init; }
        public CrewMember? CrewMember { get; init; }
        public string? IdentityUserId { get; init; }
        public bool UserAlreadyExisted { get; init; }
        public bool EmailSent { get; init; }
        public string? GeneratedPassword { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = [];

        public static CrewProvisionResult Success(CrewMember crew, string userId, bool alreadyExisted, bool emailSent, string? generatedPassword) =>
            new() { Succeeded = true, CrewMember = crew, IdentityUserId = userId, UserAlreadyExisted = alreadyExisted, EmailSent = emailSent, GeneratedPassword = generatedPassword };

        public static CrewProvisionResult Failure(params string[] errors) =>
            new() { Succeeded = false, Errors = errors };
    }
}

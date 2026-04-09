namespace AlgoaBayBMT.Shared.Models
{
    public class OperationResult
    {
        public bool Succeeded { get; init; }
        public string? Message { get; init; }
        public IReadOnlyCollection<string> Errors { get; init; } = Array.Empty<string>();

        public static OperationResult Success(string? message = null) => new() { Succeeded = true, Message = message };

        public static OperationResult Failure(params string[] errors) => new()
        {
            Succeeded = false,
            Errors = errors,
            Message = errors.FirstOrDefault()
        };
    }
}

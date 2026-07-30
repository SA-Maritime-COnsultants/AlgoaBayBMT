namespace AlgoaBayBMT.Shared.Models
{
    public class OperationResult<T> : OperationResult
    {
        public T? Data { get; init; }

        public static OperationResult<T> Success(T data, string? message = null) => new()
        {
            Succeeded = true,
            Data = data,
            Message = message
        };

        public new static OperationResult<T> Failure(params string[] errors) => new()
        {
            Succeeded = false,
            Errors = errors,
            Message = errors.FirstOrDefault()
        };
    }
}

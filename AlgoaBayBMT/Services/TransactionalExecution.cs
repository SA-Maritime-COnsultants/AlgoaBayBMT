using AlgoaBayBMT.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace AlgoaBayBMT.Services
{
    /// <summary>
    /// Runs a database transaction under the context's configured execution strategy.
    /// ApplicationDbContext is registered with EnableRetryOnFailure(), and EF Core refuses to run
    /// a manually-opened transaction outside of Database.CreateExecutionStrategy() — every
    /// service method that opens its own transaction must go through this helper or it throws
    /// InvalidOperationException at the first query inside the transaction.
    ///
    /// If <paramref name="operation"/> returns without calling transaction.CommitAsync(), the
    /// `await using` disposal rolls the transaction back, so early "failure" returns inside the
    /// delegate behave exactly like they did before this wrapper was introduced.
    /// </summary>
    internal static class TransactionalExecution
    {
        public static async Task<TResult> ExecuteAsync<TResult>(
            ApplicationDbContext dbContext,
            CancellationToken cancellationToken,
            Func<IDbContextTransaction, Task<TResult>> operation)
        {
            var strategy = dbContext.Database.CreateExecutionStrategy();
            return await strategy.ExecuteAsync(async () =>
            {
                await using var transaction = await dbContext.Database.BeginTransactionAsync(cancellationToken);
                return await operation(transaction);
            });
        }
    }
}

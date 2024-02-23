using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Npgsql;
using Npgsql.EntityFrameworkCore.PostgreSQL;

namespace Poll.DAL.Services
{
  public class PollExecutionStrategy : NpgsqlRetryingExecutionStrategy
    {
        private readonly ILogger _log;

        public PollExecutionStrategy(ExecutionStrategyDependencies dependencies) : base(dependencies, 5, TimeSpan.FromSeconds(10),
            new List<string> { "40001" })
        {
            _log = dependencies.Logger.Logger;
        }

        public string Name { get; set; }
        public TransactionPriority Priority { get; set; } = TransactionPriority.Low;
        public new int MaxRetryCount => base.MaxRetryCount;

        public override async Task<TResult> ExecuteAsync<TState, TResult>(TState state, Func<DbContext, TState, CancellationToken, Task<TResult>> operation, Func<DbContext, TState, CancellationToken, Task<ExecutionResult<TResult>>>? verifySucceeded,
            CancellationToken cancellationToken = new CancellationToken())
        {
            return await base.ExecuteAsync(state, WrapWithStatisticsCapture(operation), verifySucceeded, cancellationToken);
        }

        public new bool ShouldRetryOn(Exception e) => base.ShouldRetryOn(e);

        private Func<DbContext, TState, CancellationToken, Task<TResult>>
            WrapWithStatisticsCapture<TState, TResult>(
                Func<DbContext, TState, CancellationToken, Task<TResult>> operation)
        {
            return async (context, state, ct) =>
            {
                try
                {
                    return await operation(context, state, ct);
                }
                catch (Exception e) when (IsPostgres40001(e))
                {
                    var name = Name ?? "Unknown Operation";

                    _log.LogWarning($"Postgres 40001 occured during transaction '{name}'");

                    throw;
                }
            };
        }

        private static bool IsPostgres40001(Exception exception)
        {
            return IsPostgres40001Core(exception) ||
                   exception.InnerException != null && IsPostgres40001Core(exception.InnerException);
        }

        private static bool IsPostgres40001Core(Exception exception)
        {
            return exception is PostgresException postgresException && postgresException.SqlState == "40001";
        }

        protected override TimeSpan? GetNextDelay(Exception lastException)
        {
            if (Priority == TransactionPriority.Low)
            {
                var nextDelay = base.GetNextDelay(lastException);
                return nextDelay;
            }

            return TimeSpan.Zero;
        }
    }

    public enum TransactionPriority
    {
        High = 0,
        Low = 1,
    }
}

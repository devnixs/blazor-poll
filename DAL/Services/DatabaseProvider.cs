using Microsoft.EntityFrameworkCore;

namespace Poll.DAL.Services;

public class DatabaseProvider
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DatabaseProvider(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<TResult> Run<T,TResult>(Func<T, Task<TResult>> func) where T : notnull
    {
        using var outerScope = _serviceScopeFactory.CreateScope();
        var context = outerScope.ServiceProvider.GetService<PollContext>();

        var strategy = context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            using var innerScope = _serviceScopeFactory.CreateScope();
            var db = innerScope.ServiceProvider.GetRequiredService<PollContext>();
            // If this is true (which is the default), then a new strategy instance will be created, regardless
            // of whether we are inside a transaction or not. When false, it doesn't. It prevented propagation of the
            // transaction name for those cases, and also seems redundant because we've obviously created a strategy
            // here.
            await using var transaction = await db.Database.BeginTransactionAsync();
            var result = await func(innerScope.ServiceProvider.GetRequiredService<T>());
            await db.SaveChangesAsync();
            await transaction.CommitAsync();
            return result;
        });
    }
}
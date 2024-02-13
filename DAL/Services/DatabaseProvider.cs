using Microsoft.EntityFrameworkCore;

namespace Poll.DAL.Services;

public class DatabaseWriteContextProvider
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DatabaseWriteContextProvider(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<TResult> Write<T,TResult>(Func<T, Task<TResult>> func) where T : notnull
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

public class DatabaseReadContextProvider
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public DatabaseReadContextProvider(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task<TResult> Read<T,TResult>(Func<T, Task<TResult>> func) where T : notnull
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
            return result;
        });
    }
}
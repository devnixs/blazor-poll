using Poll.DAL.Services;
using Poll.Events;

namespace Poll.Services.PostCommitHandlers;

public class OnCacheRefreshed : PostCommitHandler<CacheRefreshedEvent, OnCacheRefreshed>
{
    private readonly AppSynchronizer _appSynchronizer;

    public OnCacheRefreshed(TransactionContext transactionContext,
        IServiceScopeFactory serviceScopeFactory,
        AppSynchronizer appSynchronizer
    ) : base(transactionContext, serviceScopeFactory)
    {
        _appSynchronizer = appSynchronizer;
    }

    protected override Task AfterCommit(CacheRefreshedEvent eventData)
    {
        _appSynchronizer.OnStateChanged();
        return Task.CompletedTask;
    }
}
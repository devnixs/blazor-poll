using Poll.DAL.Services;
using Poll.Events;

namespace Poll.Services.PostCommitHandlers;

public class OnNewAnswer : PostCommitHandler<NewAnswerEvent, OnNewAnswer>
{
    private readonly GameStateCache _gameStateCache;
    private readonly AppSynchronizer _appSynchronizer;

    public OnNewAnswer(TransactionContext transactionContext,
        IServiceScopeFactory serviceScopeFactory,
        GameStateCache gameStateCache,
        AppSynchronizer appSynchronizer
    ) : base(transactionContext, serviceScopeFactory)
    {
        _gameStateCache = gameStateCache;
        _appSynchronizer = appSynchronizer;
    }

    protected override Task AfterCommit(NewAnswerEvent eventData)
    {
        _gameStateCache.OnNewAnswer(eventData.Answer);
        _appSynchronizer.OnNewAnswer();
        return Task.CompletedTask;
    }
}
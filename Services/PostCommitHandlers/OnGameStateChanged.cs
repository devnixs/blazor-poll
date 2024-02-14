using Poll.DAL.Services;
using Poll.Events;

namespace Poll.Services.PostCommitHandlers;

public class OnGameStateChanged : PostCommitHandler<GameStateChangedEvent, OnGameStateChanged>
{
    private readonly GameStateCache _gameStateCache;

    public OnGameStateChanged(TransactionContext transactionContext,
        IServiceScopeFactory serviceScopeFactory,
        GameStateCache gameStateCache
    ) : base(transactionContext, serviceScopeFactory)
    {
        _gameStateCache = gameStateCache;
    }

    protected override async Task AfterCommit(GameStateChangedEvent eventData)
    {
        await _gameStateCache.Refresh();
    }
}
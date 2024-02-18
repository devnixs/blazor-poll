using Poll.DAL.Services;
using Poll.Events;

namespace Poll.Services.PostCommitHandlers;

public class OnPlayerNameChanged : PostCommitHandler<PlayerNameChangedEvent, OnPlayerNameChanged>
{
    private readonly GameStateCache _gameStateCache;

    public OnPlayerNameChanged(TransactionContext transactionContext,
        IServiceScopeFactory serviceScopeFactory,
        GameStateCache gameStateCache
    ) : base(transactionContext, serviceScopeFactory)
    {
        _gameStateCache = gameStateCache;
    }

    protected override async Task AfterCommit(PlayerNameChangedEvent eventData)
    {
        await _gameStateCache.PlayerNameChanged(eventData.Player);
    }
}
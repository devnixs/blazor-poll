using Poll.DAL.Services;
using Poll.Events;

namespace Poll.Services.PostCommitHandlers;

public class OnQuestionChanged : PostCommitHandler<QuestionChangedEvent, OnQuestionChanged>
{
    private readonly GameStateCache _gameStateCache;

    public OnQuestionChanged(TransactionContext transactionContext,
        IServiceScopeFactory serviceScopeFactory,
        GameStateCache gameStateCache
    ) : base(transactionContext, serviceScopeFactory)
    {
        _gameStateCache = gameStateCache;
    }

    protected override async Task AfterCommit(QuestionChangedEvent eventData)
    {
        await _gameStateCache.Refresh();
    }
}
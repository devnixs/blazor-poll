using Poll.DAL.Entities;
using Poll.DAL.Services;
using Poll.Events;

namespace Poll.Services.PostCommitHandlers;

public class OnQuestionValidated : PostCommitHandler<QuestionValidatedEvent, OnQuestionValidated>
{
    private readonly GameStateCache _gameStateCache;

    public OnQuestionValidated(TransactionContext transactionContext,
        IServiceScopeFactory serviceScopeFactory,
        GameStateCache gameStateCache
        ) : base(transactionContext, serviceScopeFactory)
    {
        _gameStateCache = gameStateCache;
    }

    protected override async Task AfterCommit(QuestionValidatedEvent eventData)
    {
        await _gameStateCache.Refresh();
    }
}
using Poll.DAL.Services;
using Poll.Services.EventHandlers;

namespace Poll.Services.PostCommitHandlers;

public abstract class PostCommitHandler<TEvent, TClass> : IEventHandler<TEvent> where TClass: PostCommitHandler<TEvent, TClass>
{
    private readonly TransactionContext _transactionContext;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private TEvent _eventData;

    public PostCommitHandler(TransactionContext transactionContext, IServiceScopeFactory serviceScopeFactory)
    {
        _transactionContext = transactionContext;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public Task OnEvent(TEvent eventData)
    {
        _eventData = eventData;
        _transactionContext.RegisterAfterCommit(AfterCommitInternal);
        return Task.CompletedTask;
    }

    protected abstract Task AfterCommit(TEvent eventData);

    private async Task AfterCommitInternal()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var handler = scope.ServiceProvider.GetRequiredService<TClass>();
        await handler.AfterCommit(_eventData);
    }
}
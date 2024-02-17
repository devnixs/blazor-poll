using Poll.Events;

namespace Poll.Services.EventHandlers;

public class PlayersCountChangedEventHandler : IEventHandler<PlayersCountChangedEvent>
{
    private readonly AppSynchronizer _appSynchronizer;

    public PlayersCountChangedEventHandler(AppSynchronizer appSynchronizer)
    {
        _appSynchronizer = appSynchronizer;
    }

    public Task OnEvent(PlayersCountChangedEvent eventData)
    {
        _appSynchronizer.OnPlayerCountChanged();
        return Task.CompletedTask;
    }
}
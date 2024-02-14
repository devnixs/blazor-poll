using Poll.Services.EventHandlers;

namespace Poll.Services;

public class DomainEvents
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<DomainEvents> _logger;

    public DomainEvents(IServiceProvider serviceProvider, ILogger<DomainEvents> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    public async Task TriggerEvent<T>(T domainEvent)
    {
        var eventHandlers = _serviceProvider.GetRequiredService<IEnumerable<IEventHandler<T>>>();

        foreach (var eventHandler in eventHandlers)
        {
            _logger.LogInformation("Triggering event handler {eventHandler}", eventHandler.GetType());
            await eventHandler.OnEvent(domainEvent);
        }
    }
}
namespace Poll.Services.EventHandlers;

public interface IEventHandler<T>
{
    Task OnEvent(T eventData);
}
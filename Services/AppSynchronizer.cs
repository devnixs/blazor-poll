using Poll.DAL.Entities;

namespace Poll.Services;

public class AppSynchronizer
{
    public readonly string Instance = Guid.NewGuid().ToString();
    
    private List<Action> StateChangedHandlers = new();

    public void SubscribeStateChanged(Action handler)
    {
        lock (StateChangedHandlers)
        {
            StateChangedHandlers.Add(handler);
        }
    }
    
    public void UnsubscribeStateChanged(Action handler)
    {
        lock (StateChangedHandlers)
        {
            StateChangedHandlers.Remove(handler);
        }
    }
    
    public virtual void OnStateChanged()
    {
        lock (StateChangedHandlers)
        {
            foreach (var handlers in StateChangedHandlers)
            {
                handlers();
            }
        }
    }
}
using Poll.DAL.Entities;

namespace Poll.Services;

public class AppSynchronizer
{
    public readonly string Instance = Guid.NewGuid().ToString();
    
    private List<Action> StateChangedHandlers = new();
    private List<Action> PlayerCountChangedHandlers = new();
    private List<Action> NewAnswerHandlers = new();

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
    
    public void SubscribePlayerCountChanged(Action handler)
    {
        lock (PlayerCountChangedHandlers)
        {
            PlayerCountChangedHandlers.Add(handler);
        }
    }
    
    public void UnsubscribePlayerCountChanged(Action handler)
    {
        lock (PlayerCountChangedHandlers)
        {
            PlayerCountChangedHandlers.Remove(handler);
        }
    }
    
    public virtual void OnPlayerCountChanged()
    {
        lock (PlayerCountChangedHandlers)
        {
            foreach (var handlers in PlayerCountChangedHandlers)
            {
                handlers();
            }
        }
    }

    public void SubscribeNewAnswer(Action handler)
    {
        lock (NewAnswerHandlers)
        {
            NewAnswerHandlers.Add(handler);
        }
    }
    
    public void UnsubscribeNewAnswer(Action handler)
    {
        lock (NewAnswerHandlers)
        {
            NewAnswerHandlers.Remove(handler);
        }
    }
    
    public virtual void OnNewAnswer()
    {
        lock (NewAnswerHandlers)
        {
            foreach (var handlers in NewAnswerHandlers)
            {
                handlers();
            }
        }
    }
}
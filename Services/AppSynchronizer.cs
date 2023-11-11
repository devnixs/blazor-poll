using Poll.DAL.Entities;

namespace Poll.Services;

public class AppSynchronizer
{
    private List<Action<Question>> QuestionChangedHandlers = new();
    private List<Action<Answer>> NewAnswerHandlers = new();

    public void SubscribeQuestionChanged(Action<Question> handler)
    {
        lock (QuestionChangedHandlers)
        {
            QuestionChangedHandlers.Add(handler);
        }
    }
    
    public void UnsubscribeQuestionChanged(Action<Question> handler)
    {
        lock (QuestionChangedHandlers)
        {
            QuestionChangedHandlers.Remove(handler);
        }
    }
    
    public virtual void OnQuestionChanged(Question q)
    {
        lock (QuestionChangedHandlers)
        {
            foreach (var handlers in QuestionChangedHandlers)
            {
                handlers(q);
            }
        }
    }
    
    
    public void SubscribeNewAnswer(Action<Answer> handler)
    {
        lock (NewAnswerHandlers)
        {
            NewAnswerHandlers.Add(handler);
        }
    }
    
    public void UnsubscribeNewAnswer(Action<Answer> handler)
    {
        lock (NewAnswerHandlers)
        {
            NewAnswerHandlers.Remove(handler);
        }
    }

    public virtual void OnNewAnswer(Answer e)
    {
        lock (NewAnswerHandlers)
        {
            foreach (var handlers in NewAnswerHandlers)
            {
                handlers(e);
            }
        }
    }
}
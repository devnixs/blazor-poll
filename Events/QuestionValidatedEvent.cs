using Poll.DAL.Entities;

namespace Poll.Events;

public class QuestionValidatedEvent
{
    
}


public class NewAnswerEvent
{
    public Answer Answer { get; set; }
}

public class QuestionChangedEvent
{
    
}

public class GameStateChangedEvent
{
    
}

public class PlayersCountChangedEvent
{
    
}
using System.ComponentModel.DataAnnotations;

namespace Poll.Components.Pages.NewGame;

public class NewGameModel
{
    [Required]
    public string Name { get; set; } = "";

    public List<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
    public Guid? WaitingImageId { get; set; }
}

public class QuestionModel
{
    public Guid Identifier { get; set; } = Guid.NewGuid();
    
    [Required]
    public string Name { get; set; } = "";

    public Guid? QuestionImageId { get; set; }
    public Guid? ResponseImageId { get; set; }

    public List<ChoiceModel> Choices { get; set; } = new List<ChoiceModel>();

    public static QuestionModel Default => new QuestionModel()
    {
        Name = "",
        QuestionImageId = null,
        ResponseImageId = null,
        Choices =
        [
            ChoiceModel.Default,
            ChoiceModel.Default,
            ChoiceModel.Default,
        ],
    };
}

public class ChoiceModel
{
    [Required]
    public string Content { get; set; } = "";

    public bool IsValid { get; set; }

    public static ChoiceModel Default => new ChoiceModel()
    {
        Content = "",
        IsValid = false,
    };

    public static ChoiceModel DefaultValid => new ChoiceModel()
    {
        Content = "",
        IsValid = true,
    };
}
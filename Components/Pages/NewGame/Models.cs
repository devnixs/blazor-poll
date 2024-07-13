using System.ComponentModel.DataAnnotations;

namespace Poll.Components.Pages.NewGame;

public class NewGameModel
{
    [Required]
    public string Name { get; set; } = "";

    public List<QuestionModel> Questions { get; set; } = new List<QuestionModel>();
}

public class QuestionModel
{
    [Required]
    public string Name { get; set; } = "";

    public string QuestionImage { get; set; } = "";
    public string ResponseImage { get; set; } = "";

    public List<ChoiceModel> Choices { get; set; } = new List<ChoiceModel>();

    public static QuestionModel Default => new QuestionModel()
    {
        Name = "",
        QuestionImage = "",
        ResponseImage = "",
        Choices = new List<ChoiceModel>
        {
            ChoiceModel.DefaultValid,
            ChoiceModel.Default,
            ChoiceModel.Default,
        }
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
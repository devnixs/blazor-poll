using FluentValidation;

namespace Poll.Components.Pages.NewGame;

public class NewGameValidator : AbstractValidator<NewGameModel>
{
    public NewGameValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Requis");
        RuleForEach(x => x.Questions).SetValidator(new QuestionValidator());
    }
}

public class QuestionValidator : AbstractValidator<QuestionModel>
{
    public QuestionValidator()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Requis");
        RuleFor(x => x.QuestionImage).Must(i=> string.IsNullOrEmpty(i) || i.StartsWith("https://")).WithMessage("Doit être une url valide commançant par https://");
        RuleFor(x => x.ResponseImage).Must(i=> string.IsNullOrEmpty(i) || i.StartsWith("https://")).WithMessage("Doit être une url valide commançant par https://");
        RuleForEach(x => x.Choices).SetValidator(new ChoiceValidator());
        RuleFor(x => x.Choices).Must(i=> i.Count(j=>j.IsValid) > 0).WithMessage("Au moins un choix doit être valide.");
        RuleFor(x => x.Choices).Must(i=> i.Count(j=>j.IsValid) < 2).WithMessage("Seulement un choix doit être valide.");
    }
}

public class ChoiceValidator : AbstractValidator<ChoiceModel>
{
    public ChoiceValidator()
    {
        RuleFor(x => x.Content).NotEmpty().WithMessage("Requis");
    }
}
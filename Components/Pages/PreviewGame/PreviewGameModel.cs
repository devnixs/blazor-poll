using System.Text.RegularExpressions;
using FluentValidation;

namespace Poll.Components.Pages.PreviewGame;

public class PreviewGameModel
{
    public string GameIdentifier { get; set; }
    public string QuestionDelay { get; set; }
}

public class GamePreviewValidator : AbstractValidator<PreviewGameModel>
{
    public GamePreviewValidator()
    {
        RuleFor(x => x.GameIdentifier)
            .Must(i => string.IsNullOrEmpty(i) || Guid.TryParse(i, out _))
            .WithMessage("N'est pas un identifiant valide.");

        RuleFor(x => x.QuestionDelay)
            .Must(i => string.IsNullOrEmpty(i) || (int.TryParse(i, out var integer) && integer > 0))
            .WithMessage("N'est pas un nombre valide.");
    }
}
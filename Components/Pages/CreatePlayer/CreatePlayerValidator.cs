using System.Text.RegularExpressions;
using FluentValidation;
using Poll.DAL.Entities;

namespace Poll.Components.Pages.CreatePlayer;

public class CreatePlayerValidator : AbstractValidator<Player>
{
    public CreatePlayerValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Ne doit pas être vide.")
            .Must(i => string.IsNullOrEmpty(i) || i.Length >= 3)
            .WithMessage("Doit faire au moins 3 charactères.")
            .Must(i => string.IsNullOrEmpty(i) || i.Length < 3 || Regex.Match(i, @"^[A-Za-zéèêëï'0-9 -]{3,}$").Success)
            .WithMessage("Ce prénom contient des charactères invalides.");
    }
}
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
            .Must(i => string.IsNullOrEmpty(i) || Regex.Match(i, @"^[A-Za-zéèêëï' -]{3,}$").Success)
            .WithMessage("Ce prénom contient des charactères invalides.");
    }
}
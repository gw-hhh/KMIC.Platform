using FluentValidation;

namespace Kmic.Identity.Application.Users.Grants.SetAnchors;

internal sealed class SetGrantAnchorsCommandValidator : AbstractValidator<SetGrantAnchorsCommand>
{
    public SetGrantAnchorsCommandValidator()
    {
        RuleFor(c => c.Anchors).NotNull().Must(a => a.Count <= 200).WithMessage("锚点最多 200 个");
    }
}

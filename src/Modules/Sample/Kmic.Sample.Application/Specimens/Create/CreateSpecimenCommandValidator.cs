using FluentValidation;

namespace Kmic.Sample.Application.Specimens.Create;

internal sealed class CreateSpecimenCommandValidator : AbstractValidator<CreateSpecimenCommand>
{
    public CreateSpecimenCommandValidator()
    {
        RuleFor(c => c.Title).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Items).NotEmpty().Must(items => items.Count <= 50).WithMessage("明细最多 50 项");
        RuleForEach(c => c.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Name).NotEmpty().MaximumLength(50);
            item.RuleFor(i => i.Quantity).InclusiveBetween(1, 9999);
        });
    }
}

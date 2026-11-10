using FluentValidation;

namespace Kmic.BuildingBlocks.Application.Tests.Fixtures;

internal sealed class SubmitThingCommandValidator : AbstractValidator<SubmitThingCommand>
{
    public SubmitThingCommandValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item => item.RuleFor(i => i.Quantity).GreaterThan(0));
    }
}

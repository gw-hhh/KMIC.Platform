using FluentValidation;

namespace Kmic.Operations.Application.Messages.Rebuilds;

internal sealed class TriggerRebuildCommandValidator : AbstractValidator<TriggerRebuildCommand>
{
    public TriggerRebuildCommandValidator()
    {
        RuleFor(c => c.Subscription).NotEmpty().MaximumLength(200);
    }
}

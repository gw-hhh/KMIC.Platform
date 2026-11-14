using FluentValidation;

namespace Kmic.Operations.Application.Messages.Routes;

internal sealed class FinalizeRouteCommandValidator : AbstractValidator<FinalizeRouteCommand>
{
    public FinalizeRouteCommandValidator()
    {
        RuleFor(c => c.MessageType).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Subscription).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(500);
    }
}

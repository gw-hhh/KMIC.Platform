using FluentValidation;

namespace Kmic.Operations.Application.Messages.Routes;

internal sealed class ReactivateRouteCommandValidator : AbstractValidator<ReactivateRouteCommand>
{
    public ReactivateRouteCommandValidator()
    {
        RuleFor(c => c.MessageType).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Subscription).NotEmpty().MaximumLength(200);
    }
}

using FluentValidation;

namespace Kmic.Operations.Application.Messages.Routes;

internal sealed class RedeliverCommandValidator : AbstractValidator<RedeliverCommand>
{
    public RedeliverCommandValidator()
    {
        RuleFor(c => c.SourceModule).NotEmpty().MaximumLength(100);
        RuleFor(c => c.MessageType).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Subscription).NotEmpty().MaximumLength(200);
        RuleFor(c => c.FromSeq).GreaterThanOrEqualTo(1);
        RuleFor(c => c.ToSeq).GreaterThanOrEqualTo(c => c.FromSeq).WithMessage("结束序号不能小于起始序号");
    }
}

using FluentValidation;

namespace Kmic.Operations.Application.Messages.Retry;

internal sealed class RetryMessagesCommandValidator : AbstractValidator<RetryMessagesCommand>
{
    public RetryMessagesCommandValidator()
    {
        RuleFor(c => c.Messages).NotEmpty().Must(m => m.Count <= MessageLimits.MaxPerRequest).WithMessage($"一次最多处理 {MessageLimits.MaxPerRequest} 条");
        RuleForEach(c => c.Messages).ChildRules(message =>
        {
            message.RuleFor(m => m.Module).NotEmpty().MaximumLength(100);
            message.RuleFor(m => m.Id).NotEmpty();
        });
    }
}

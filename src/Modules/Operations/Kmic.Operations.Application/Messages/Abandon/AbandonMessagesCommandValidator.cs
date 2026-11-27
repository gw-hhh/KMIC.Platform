using FluentValidation;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages.Abandon;

internal sealed class AbandonMessagesCommandValidator : AbstractValidator<AbandonMessagesCommand>
{
    public AbandonMessagesCommandValidator()
    {
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(500);
        RuleFor(c => c.Messages).NotEmpty().Must(m => m.Count <= MessageLimits.MaxPerRequest).WithMessage($"一次最多处理 {MessageLimits.MaxPerRequest} 条");
        RuleForEach(c => c.Messages).ChildRules(message =>
        {
            message.RuleFor(m => m.Module).NotEmpty().MaximumLength(100);
            message.RuleFor(m => m.Id).NotEmpty();
            message.RuleFor(m => m.Box).Must(box => box == MessageBoxKind.Inbox).WithMessage("仅支持放弃收件箱死信，发件箱死信只能重试");
        });
    }
}

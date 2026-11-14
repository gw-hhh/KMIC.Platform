using FluentValidation;

namespace Kmic.Operations.Application.Audit.Export;

internal sealed class ExportAuditCommandValidator : AbstractValidator<ExportAuditCommand>
{
    public ExportAuditCommandValidator()
    {
        RuleFor(c => c.From).NotEmpty().WithMessage("必须给起始时间");
        RuleFor(c => c.To).NotEmpty().WithMessage("必须给结束时间");
        RuleFor(c => c.To).GreaterThan(c => c.From).When(c => c.From != default && c.To != default).WithMessage("结束时间必须晚于起始时间");
        RuleFor(c => c.Module).MaximumLength(50);
        RuleFor(c => c.Action).MaximumLength(200);
        RuleFor(c => c.ResourceType).MaximumLength(100);
    }
}

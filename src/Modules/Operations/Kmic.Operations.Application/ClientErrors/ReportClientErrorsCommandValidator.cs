using FluentValidation;

namespace Kmic.Operations.Application.ClientErrors;

internal sealed class ReportClientErrorsCommandValidator : AbstractValidator<ReportClientErrorsCommand>
{
    public ReportClientErrorsCommandValidator()
    {
        RuleFor(c => c.Errors).NotEmpty();
        RuleForEach(c => c.Errors).ChildRules(error =>
        {
            error.RuleFor(e => e.Type).NotEmpty().MaximumLength(100);
            error.RuleFor(e => e.Message).NotEmpty().MaximumLength(1000);
            error.RuleFor(e => e.Stack).MaximumLength(8000);
            error.RuleFor(e => e.RouteName).MaximumLength(200);
            error.RuleFor(e => e.AppVersion).MaximumLength(50);
            error.RuleFor(e => e.TraceId).Must(IsTraceId).WithMessage("追踪 ID 应是 32 位十六进制");
        });
    }

    private static bool IsTraceId(string? id) => id is null || (id.Length == 32 && id.All(char.IsAsciiHexDigitLower));
}

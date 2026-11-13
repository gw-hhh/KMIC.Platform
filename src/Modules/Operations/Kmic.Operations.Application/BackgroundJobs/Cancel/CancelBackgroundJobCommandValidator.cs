using FluentValidation;

namespace Kmic.Operations.Application.BackgroundJobs.Cancel;

internal sealed class CancelBackgroundJobCommandValidator : AbstractValidator<CancelBackgroundJobCommand>
{
    public CancelBackgroundJobCommandValidator()
    {
        RuleFor(c => c.Module).NotEmpty().MaximumLength(100);
        RuleFor(c => c.JobId).NotEmpty();
    }
}

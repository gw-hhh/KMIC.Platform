using FluentValidation;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

internal sealed class CancelMyJobCommandValidator : AbstractValidator<CancelMyJobCommand>
{
    public CancelMyJobCommandValidator()
    {
        RuleFor(c => c.Module).NotEmpty().MaximumLength(100);
        RuleFor(c => c.JobId).NotEmpty();
    }
}

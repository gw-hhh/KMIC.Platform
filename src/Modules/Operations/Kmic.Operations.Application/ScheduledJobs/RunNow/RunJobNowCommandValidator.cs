using FluentValidation;

namespace Kmic.Operations.Application.ScheduledJobs.RunNow;

internal sealed class RunJobNowCommandValidator : AbstractValidator<RunJobNowCommand>
{
    public RunJobNowCommandValidator() => RuleFor(c => c.JobName).NotEmpty().MaximumLength(200);
}

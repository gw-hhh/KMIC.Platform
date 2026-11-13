using FluentValidation;

namespace Kmic.Operations.Application.ScheduledJobs.Set;

internal sealed class SetJobOverrideCommandValidator : AbstractValidator<SetJobOverrideCommand>
{
    public SetJobOverrideCommandValidator()
    {
        RuleFor(c => c.JobName).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Cron).NotEmpty().MaximumLength(120).When(c => c.Cron is not null);
    }
}

using FluentValidation;

namespace Kmic.Operations.Application.ScheduledJobs.Restore;

internal sealed class RestoreJobDefaultCommandValidator : AbstractValidator<RestoreJobDefaultCommand>
{
    public RestoreJobDefaultCommandValidator() => RuleFor(c => c.JobName).NotEmpty().MaximumLength(200);
}

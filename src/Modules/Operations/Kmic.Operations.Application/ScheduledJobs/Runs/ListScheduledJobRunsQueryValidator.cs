using FluentValidation;

namespace Kmic.Operations.Application.ScheduledJobs.Runs;

internal sealed class ListScheduledJobRunsQueryValidator : AbstractValidator<ListScheduledJobRunsQuery>
{
    public ListScheduledJobRunsQueryValidator() => RuleFor(q => q.JobName).NotEmpty().MaximumLength(200);
}

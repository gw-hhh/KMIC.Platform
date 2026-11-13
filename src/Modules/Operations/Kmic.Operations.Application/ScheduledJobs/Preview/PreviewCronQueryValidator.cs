using FluentValidation;

namespace Kmic.Operations.Application.ScheduledJobs.Preview;

internal sealed class PreviewCronQueryValidator : AbstractValidator<PreviewCronQuery>
{
    public PreviewCronQueryValidator() => RuleFor(q => q.Cron).NotEmpty().MaximumLength(120);
}

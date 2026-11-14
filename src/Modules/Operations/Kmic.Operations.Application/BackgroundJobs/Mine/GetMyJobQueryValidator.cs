using FluentValidation;

namespace Kmic.Operations.Application.BackgroundJobs.Mine;

internal sealed class GetMyJobQueryValidator : AbstractValidator<GetMyJobQuery>
{
    public GetMyJobQueryValidator()
    {
        RuleFor(q => q.Module).NotEmpty().MaximumLength(100);
        RuleFor(q => q.JobId).NotEmpty();
    }
}

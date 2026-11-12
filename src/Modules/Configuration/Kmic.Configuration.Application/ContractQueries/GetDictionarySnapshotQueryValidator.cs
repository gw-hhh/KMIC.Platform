using FluentValidation;

namespace Kmic.Configuration.Application.ContractQueries;

internal sealed class GetDictionarySnapshotQueryValidator : AbstractValidator<GetDictionarySnapshotQuery>
{
    public GetDictionarySnapshotQueryValidator() => RuleFor(q => q.TypeCode).NotEmpty().MaximumLength(100);
}

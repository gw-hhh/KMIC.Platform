using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.Identity.Application.ContractQueries;

internal sealed class GetRoleReferencesQueryValidator : AbstractValidator<GetRoleReferencesQuery>
{
    public GetRoleReferencesQueryValidator() =>
        RuleFor(q => q.Ids)
            .NotNull()
            .Must(ids => ids.Count <= ContractBatch.DefaultSize)
            .WithErrorCode("Range")
            .WithMessage($"一次最多解析 {ContractBatch.DefaultSize} 个角色");
}

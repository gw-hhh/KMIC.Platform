using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.Identity.Application.ContractQueries;

internal sealed class GetUserContactsQueryValidator : AbstractValidator<GetUserContactsQuery>
{
    public GetUserContactsQueryValidator() =>
        RuleFor(q => q.Ids)
            .NotNull()
            .Must(ids => ids.Count <= ContractBatch.DefaultSize)
            .WithErrorCode("Range")
            .WithMessage($"一次最多查询 {ContractBatch.DefaultSize} 个用户的联系方式");
}

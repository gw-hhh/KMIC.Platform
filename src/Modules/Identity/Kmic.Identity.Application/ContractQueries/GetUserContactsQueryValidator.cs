using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 校验单批用户联系方式查询的 ID 数量
/// </summary>
internal sealed class GetUserContactsQueryValidator : AbstractValidator<GetUserContactsQuery>
{
    public GetUserContactsQueryValidator() =>
        RuleFor(q => q.Ids)
            .NotNull()
            .Must(ids => ids.Count <= ContractBatch.DefaultSize)
            .WithErrorCode("Range")
            .WithMessage($"一次最多取 {ContractBatch.DefaultSize} 个用户的联系方式。");
}

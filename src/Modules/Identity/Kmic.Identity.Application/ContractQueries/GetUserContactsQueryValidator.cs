using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 校验 <see cref="GetUserContactsQuery"/>：一次的 ID 数量有上限，适配器负责分批
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

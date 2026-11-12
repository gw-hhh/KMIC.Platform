using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.Identity.Application.ContractQueries;

/// <summary>
/// 校验 <see cref="GetUserReferencesQuery"/>：一次的 ID 数量有上限，适配器负责分批
/// </summary>
internal sealed class GetUserReferencesQueryValidator : AbstractValidator<GetUserReferencesQuery>
{
    public GetUserReferencesQueryValidator() =>
        RuleFor(q => q.Ids)
            .NotNull()
            .Must(ids => ids.Count <= ContractBatch.DefaultSize)
            .WithErrorCode("Range")
            .WithMessage($"一次最多解析 {ContractBatch.DefaultSize} 个用户。");
}

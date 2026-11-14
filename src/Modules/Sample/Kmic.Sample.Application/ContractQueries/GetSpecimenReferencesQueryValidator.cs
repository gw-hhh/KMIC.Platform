using FluentValidation;
using Kmic.BuildingBlocks.Application.Messaging;

namespace Kmic.Sample.Application.ContractQueries;

/// <summary>
/// 示例引用查询的参数校验
/// </summary>
internal sealed class GetSpecimenReferencesQueryValidator : AbstractValidator<GetSpecimenReferencesQuery>
{
    public GetSpecimenReferencesQueryValidator() =>
        RuleFor(q => q.Ids)
            .NotNull()
            .Must(ids => ids.Count <= ContractBatch.DefaultSize)
            .WithErrorCode("Range")
            .WithMessage($"一次最多解析 {ContractBatch.DefaultSize} 个示例。");
}

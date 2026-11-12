using FluentValidation;

namespace Kmic.Configuration.Application.Dictionaries.Get;

internal sealed class GetDictionariesQueryValidator : AbstractValidator<GetDictionariesQuery>
{
    public const int MaxTypes = 20;

    public GetDictionariesQueryValidator()
    {
        RuleFor(q => q.Types).NotNull().Must(types => types.Count is >= 1 and <= MaxTypes).WithMessage($"一次要 1 到 {MaxTypes} 个字典类型");
        RuleForEach(q => q.Types).NotEmpty().MaximumLength(100);
    }
}

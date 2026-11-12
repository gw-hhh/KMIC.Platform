using FluentValidation;

namespace Kmic.Configuration.Application.Dictionaries.Items;

internal sealed class AddDictionaryItemCommandValidator : AbstractValidator<AddDictionaryItemCommand>
{
    public AddDictionaryItemCommandValidator()
    {
        RuleFor(c => c.TypeCode).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.ParentCode).MaximumLength(50);
    }
}

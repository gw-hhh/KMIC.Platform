using FluentValidation;

namespace Kmic.Configuration.Application.Dictionaries.Items;

internal sealed class RenameDictionaryItemCommandValidator : AbstractValidator<RenameDictionaryItemCommand>
{
    public RenameDictionaryItemCommandValidator()
    {
        RuleFor(c => c.TypeCode).NotEmpty().MaximumLength(100);
        RuleFor(c => c.Code).NotEmpty().MaximumLength(50);
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
    }
}

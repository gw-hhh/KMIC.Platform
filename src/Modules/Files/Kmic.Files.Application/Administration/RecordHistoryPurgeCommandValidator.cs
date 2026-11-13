using FluentValidation;

namespace Kmic.Files.Application.Administration;

internal sealed class RecordHistoryPurgeCommandValidator : AbstractValidator<RecordHistoryPurgeCommand>
{
    public RecordHistoryPurgeCommandValidator()
    {
        RuleFor(c => c.Note).NotEmpty().MaximumLength(500);
    }
}

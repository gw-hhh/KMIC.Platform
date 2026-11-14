using FluentValidation;

namespace Kmic.Files.Application.Administration;

internal sealed class ResolveStorageIssueCommandValidator : AbstractValidator<ResolveStorageIssueCommand>
{
    public ResolveStorageIssueCommandValidator()
    {
        RuleFor(c => c.Note).NotEmpty().MaximumLength(500);
    }
}

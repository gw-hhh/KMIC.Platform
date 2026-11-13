using FluentValidation;

namespace Kmic.Files.Application.Administration;

internal sealed class PurgeFileCommandValidator : AbstractValidator<PurgeFileCommand>
{
    public PurgeFileCommandValidator()
    {
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(500);
    }
}

using FluentValidation;

namespace Kmic.Files.Application.Uploads.Start;

internal sealed class StartUploadCommandValidator : AbstractValidator<StartUploadCommand>
{
    public StartUploadCommandValidator()
    {
        RuleFor(c => c.Profile).NotEmpty().MaximumLength(200);
        RuleFor(c => c.FileName).NotEmpty().MaximumLength(1000);
        RuleFor(c => c.DeclaredSize).GreaterThanOrEqualTo(0);
    }
}

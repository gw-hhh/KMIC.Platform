using FluentValidation;

namespace Kmic.Files.Application.Uploads.Complete;

internal sealed class CompleteUploadCommandValidator : AbstractValidator<CompleteUploadCommand>
{
    public CompleteUploadCommandValidator()
    {
        RuleFor(c => c.FileId).NotEmpty();
        RuleFor(c => c.FileType).NotEmpty().MaximumLength(50);
        RuleFor(c => c.ContentType).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Size).GreaterThan(0);
        RuleFor(c => c.Sha256).NotEmpty().Length(64);
        RuleFor(c => c.ObjectVersionId).MaximumLength(200);
    }
}

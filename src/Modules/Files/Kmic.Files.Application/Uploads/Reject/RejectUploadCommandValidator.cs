using FluentValidation;

namespace Kmic.Files.Application.Uploads.Reject;

internal sealed class RejectUploadCommandValidator : AbstractValidator<RejectUploadCommand>
{
    public RejectUploadCommandValidator()
    {
        RuleFor(c => c.FileId).NotEmpty();
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(500);
    }
}

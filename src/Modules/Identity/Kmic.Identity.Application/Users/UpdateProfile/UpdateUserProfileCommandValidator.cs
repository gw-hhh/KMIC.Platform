using FluentValidation;

namespace Kmic.Identity.Application.Users.UpdateProfile;

internal sealed class UpdateUserProfileCommandValidator : AbstractValidator<UpdateUserProfileCommand>
{
    public UpdateUserProfileCommandValidator()
    {
        RuleFor(c => c.Account).NotEmpty().MaximumLength(100);
        RuleFor(c => c.DisplayName).NotEmpty().MaximumLength(100);
        RuleFor(c => c.EnglishName).MaximumLength(100);
        RuleFor(c => c.EmployeeNo).MaximumLength(50);
        RuleFor(c => c.Mobile).MaximumLength(20);
        RuleFor(c => c.Email).MaximumLength(200);
    }
}

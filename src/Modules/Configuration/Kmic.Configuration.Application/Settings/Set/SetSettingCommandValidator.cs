using System.Text.Json;
using FluentValidation;

namespace Kmic.Configuration.Application.Settings.Set;

internal sealed class SetSettingCommandValidator : AbstractValidator<SetSettingCommand>
{
    public SetSettingCommandValidator()
    {
        RuleFor(c => c.Key).NotEmpty().MaximumLength(200);
        RuleFor(c => c.Reason).NotEmpty().MaximumLength(500);
        RuleFor(c => c.Value).Must(v => v.ValueKind == JsonValueKind.Object).WithMessage("值必须是 JSON 对象");
    }
}

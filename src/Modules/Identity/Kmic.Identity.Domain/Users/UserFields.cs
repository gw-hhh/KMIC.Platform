using System.Net.Mail;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Domain.Errors;

namespace Kmic.Identity.Domain.Users;

/// <summary>
/// 用户资料字段的规范化与校验
/// </summary>
internal static class UserFields
{
    public static Result<string> Account(string? input)
    {
        var account = input?.Trim();
        return string.IsNullOrEmpty(account) || account.Length > 100 || HasWhitespaceOrControl(account) ? IdentityErrors.AccountInvalid : account;
    }

    public static Result<string> DisplayName(string? input)
    {
        var name = input?.Trim();
        return string.IsNullOrEmpty(name) || name.Length > 100 || name.Any(char.IsControl) ? IdentityErrors.DisplayNameInvalid : name;
    }

    public static Result<string?> EnglishName(string? input)
    {
        var name = Blank(input);
        return name is { Length: > 100 } || name?.Any(char.IsControl) == true ? IdentityErrors.DisplayNameInvalid : name;
    }

    public static Result<string?> EmployeeNo(string? input)
    {
        var number = Blank(input);
        return number is not null && (number.Length > 50 || HasWhitespaceOrControl(number)) ? IdentityErrors.EmployeeNoInvalid : number;
    }

    public static Result<string?> Mobile(string? input) =>
        Blank(input) is { } value ? MobileNumbers.Normalize(value).Map<string?>(m => m) : (string?)null;

    public static Result<string?> Email(string? input)
    {
        var email = Blank(input);
        if (email is null)
        {
            return (string?)null;
        }

        return email.Length > 200 || !MailAddress.TryCreate(email, out var parsed) || !string.Equals(parsed.Address, email, StringComparison.Ordinal)
            ? IdentityErrors.EmailInvalid
            : email;
    }

    private static string? Blank(string? input) => string.IsNullOrWhiteSpace(input) ? null : input.Trim();

    private static bool HasWhitespaceOrControl(string text) => text.Any(c => char.IsWhiteSpace(c) || char.IsControl(c));
}

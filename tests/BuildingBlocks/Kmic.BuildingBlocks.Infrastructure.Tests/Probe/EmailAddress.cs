using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.ValueObjects;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe;

/// <summary>
/// 邮箱地址
/// </summary>
internal sealed record EmailAddress : ISingleValueObject<string>
{
    private static readonly Error _invalid = Error.Validation("Probe.EmailInvalid", "邮箱格式不正确");

    private EmailAddress(string value) => Value = value;

    public string Value { get; private init; }

    public static Result<EmailAddress> Create(string input)
    {
        var normalized = input.Trim().ToUpperInvariant();
        return normalized.Contains('@', StringComparison.Ordinal) ? new EmailAddress(normalized) : _invalid;
    }
}

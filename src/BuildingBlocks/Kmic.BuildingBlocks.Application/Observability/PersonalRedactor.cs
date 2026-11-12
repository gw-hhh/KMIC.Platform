using Microsoft.Extensions.Compliance.Redaction;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// Personal：部分遮盖，规则见 <see cref="PartialMask" />
/// </summary>
public sealed class PersonalRedactor : Redactor
{
    /// <inheritdoc />
    public override int GetRedactedLength(ReadOnlySpan<char> input) => input.IsEmpty ? 0 : PartialMask.Of(input.ToString()).Length;

    /// <inheritdoc />
    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        var masked = PartialMask.Of(source.ToString());
        masked.CopyTo(destination);
        return masked.Length;
    }
}

using Microsoft.Extensions.Compliance.Redaction;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// Internal：原样输出
/// </summary>
public sealed class InternalRedactor : Redactor
{
    /// <inheritdoc />
    public override int GetRedactedLength(ReadOnlySpan<char> input) => input.Length;

    /// <inheritdoc />
    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        source.CopyTo(destination);
        return source.Length;
    }
}

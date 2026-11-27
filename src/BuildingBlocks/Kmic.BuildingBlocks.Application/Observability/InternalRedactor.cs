using Microsoft.Extensions.Compliance.Redaction;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>Internal 数据脱敏器，原样输出内容。</summary>
public sealed class InternalRedactor : Redactor
{
    public override int GetRedactedLength(ReadOnlySpan<char> input) => input.Length;

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        source.CopyTo(destination);
        return source.Length;
    }
}

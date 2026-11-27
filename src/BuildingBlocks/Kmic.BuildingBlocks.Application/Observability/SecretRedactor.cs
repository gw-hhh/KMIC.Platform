using Microsoft.Extensions.Compliance.Redaction;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>敏感数据脱敏器，统一替换为固定长度的 *** 以避免泄露原文长度。</summary>
public sealed class SecretRedactor : Redactor
{
    public const string Mask = "***";

    public override int GetRedactedLength(ReadOnlySpan<char> input) => Mask.Length;

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        Mask.CopyTo(destination);
        return Mask.Length;
    }
}

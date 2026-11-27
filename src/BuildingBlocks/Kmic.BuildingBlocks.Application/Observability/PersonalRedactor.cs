using Microsoft.Extensions.Compliance.Redaction;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>Personal 数据脱敏器，按 <see cref="PartialMask" /> 规则部分遮盖。</summary>
public sealed class PersonalRedactor : Redactor
{
    public override int GetRedactedLength(ReadOnlySpan<char> input) => input.IsEmpty ? 0 : PartialMask.Of(input.ToString()).Length;

    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        var masked = PartialMask.Of(source.ToString());
        masked.CopyTo(destination);
        return masked.Length;
    }
}

using Microsoft.Extensions.Compliance.Redaction;

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// Secret：整个值换成 ***，不暴露长度
/// </summary>
public sealed class SecretRedactor : Redactor
{
    /// <summary>
    /// 输出的内容
    /// </summary>
    public const string Mask = "***";

    /// <inheritdoc />
    public override int GetRedactedLength(ReadOnlySpan<char> input) => Mask.Length;

    /// <inheritdoc />
    public override int Redact(ReadOnlySpan<char> source, Span<char> destination)
    {
        Mask.CopyTo(destination);
        return Mask.Length;
    }
}

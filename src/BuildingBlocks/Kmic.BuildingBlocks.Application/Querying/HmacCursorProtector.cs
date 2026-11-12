using System.Buffers.Text;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Kmic.BuildingBlocks.Application.Querying;

/// <summary>
/// <see cref="ICursorProtector" /> 的实现：排序键与 ID 拼成文本，Base64Url 编码，后面跟 HMAC-SHA256 签名
/// </summary>
public sealed class HmacCursorProtector : ICursorProtector
{
    private const char Separator = '.';
    private const int MinKeyBytes = 32;

    private readonly byte[] _key;

    /// <summary>
    /// 用配置里的密钥；没有配置时随机生成一个，只在本进程内有效
    /// </summary>
    public HmacCursorProtector(IOptions<CursorOptions> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _key = options.Value.SigningKey is { Length: > 0 } configured
            ? ReadKey(configured)
            : RandomNumberGenerator.GetBytes(MinKeyBytes);
    }

    /// <inheritdoc />
    public string Protect(string key, Guid id)
    {
        ArgumentNullException.ThrowIfNull(key);

        // 排序键在前、ID 在后：键里不会有换行，用换行分开
        var payload = Base64Url.EncodeToString(Encoding.UTF8.GetBytes(string.Create(CultureInfo.InvariantCulture, $"{key}\n{id:N}")));
        return payload + Separator + Base64Url.EncodeToString(Sign(payload));
    }

    /// <inheritdoc />
    public bool TryUnprotect(string cursor, out string key, out Guid id)
    {
        key = string.Empty;
        id = Guid.Empty;
        if (string.IsNullOrEmpty(cursor) || cursor.Length > 512)
        {
            return false;
        }

        var parts = cursor.Split(Separator);
        if (parts.Length != 2 || !TryDecode(parts[1], out var signature) || !CryptographicOperations.FixedTimeEquals(signature, Sign(parts[0])))
        {
            return false;
        }

        if (!TryDecode(parts[0], out var payload))
        {
            return false;
        }

        var text = Encoding.UTF8.GetString(payload);
        var newline = text.IndexOf('\n', StringComparison.Ordinal);
        if (newline <= 0 || !Guid.TryParseExact(text.AsSpan(newline + 1), "N", out id))
        {
            return false;
        }

        key = text[..newline];
        return true;
    }

    private static byte[] ReadKey(string configured)
    {
        try
        {
            var bytes = Convert.FromBase64String(configured);
            return bytes.Length >= MinKeyBytes
                ? bytes
                : throw new InvalidOperationException($"Kmic:Paging:SigningKey 至少 {MinKeyBytes} 字节（Base64 解码后）。");
        }
        catch (FormatException exception)
        {
            throw new InvalidOperationException("Kmic:Paging:SigningKey 不是合法的 Base64。", exception);
        }
    }

    private static bool TryDecode(string text, out byte[] bytes)
    {
        try
        {
            bytes = Base64Url.DecodeFromChars(text);
            return true;
        }
        catch (FormatException)
        {
            bytes = [];
            return false;
        }
    }

    private byte[] Sign(string payload) => HMACSHA256.HashData(_key, Encoding.ASCII.GetBytes(payload));
}

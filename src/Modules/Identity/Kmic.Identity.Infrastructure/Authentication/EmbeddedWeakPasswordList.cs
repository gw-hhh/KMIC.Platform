using System.Collections.Frozen;
using Kmic.Identity.Application.Authentication;

namespace Kmic.Identity.Infrastructure.Authentication;

/// <summary>
/// 随模块嵌入的常见弱密码清单
/// </summary>
internal sealed class EmbeddedWeakPasswordList : IWeakPasswordList
{
    private const string ResourceName = "Kmic.Identity.Infrastructure.Authentication.weak-passwords.txt";

    private static readonly Lazy<FrozenSet<string>> _passwords = new(Load);

    public bool Contains(string password)
    {
        ArgumentNullException.ThrowIfNull(password);
        return _passwords.Value.Contains(password);
    }

    private static FrozenSet<string> Load()
    {
        using var stream = typeof(EmbeddedWeakPasswordList).Assembly.GetManifestResourceStream(ResourceName)
            ?? throw new InvalidOperationException($"找不到嵌入的资源 {ResourceName}。");
        using var reader = new StreamReader(stream);
        var lines = new List<string>();
        while (reader.ReadLine() is { } line)
        {
            if (line.Length > 0 && line[0] != '#')
            {
                lines.Add(line.Trim());
            }
        }

        return lines.ToFrozenSet(StringComparer.OrdinalIgnoreCase);
    }
}

namespace Kmic.Files.Application.Uploads;

/// <summary>
/// 清理用于显示的文件名，移除路径和控制字符并限制长度
/// </summary>
internal static class FileNames
{
    /// <summary>
    /// 文件名最大字符数
    /// </summary>
    public const int MaxLength = 200;

    private static readonly char[] _separators = ['/', '\\'];

    /// <summary>
    /// 清理文件名
    /// </summary>
    /// <returns>清理后的文件名；为空、"." 或 ".." 时返回 null</returns>
    public static string? Sanitize(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        var slash = fileName.LastIndexOfAny(_separators);
        var name = slash >= 0 ? fileName[(slash + 1)..] : fileName;
        name = string.Concat(name.Where(c => !char.IsControl(c) && !IsBidiControl(c))).Trim();
        if (name.Length == 0 || name is "." or "..")
        {
            return null;
        }

        if (name.Length <= MaxLength)
        {
            return name;
        }

        var extension = Path.GetExtension(name);
        var keep = MaxLength - Math.Min(extension.Length, 20);
        return name[..keep] + (extension.Length <= 20 ? extension : string.Empty);
    }

    // 方向控制字符（如 U+202E）能把“fdp.exe”显示成“exe.pdf”，用来伪装扩展名
    private static bool IsBidiControl(char c) => c is >= '‪' and <= '‮' or >= '⁦' and <= '⁩' or '‎' or '‏' or '؜';
}

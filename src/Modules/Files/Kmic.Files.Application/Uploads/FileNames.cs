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
    /// 清理文件名，超长时截断；扩展名不超过 20 个字符时保留，结果为空、"." 或 ".." 时返回 null
    /// </summary>
    /// <param name="fileName">客户端给的文件名</param>
    public static string? Sanitize(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        var slash = fileName.LastIndexOfAny(_separators);
        var name = slash >= 0 ? fileName[(slash + 1)..] : fileName;
        name = string.Concat(name.Where(c => !char.IsControl(c))).Trim();
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
}

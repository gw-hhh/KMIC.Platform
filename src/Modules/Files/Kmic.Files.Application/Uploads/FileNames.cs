namespace Kmic.Files.Application.Uploads;

/// <summary>
/// 文件名的整理：只作显示用，去掉路径和控制字符，长度有上限
/// </summary>
internal static class FileNames
{
    /// <summary>
    /// 保存下来的文件名最长多少个字符
    /// </summary>
    public const int MaxLength = 200;

    private static readonly char[] _separators = ['/', '\\'];

    /// <summary>
    /// 去掉路径部分与控制字符，超长时截断主干、保留扩展名；整理完是空的返回空
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

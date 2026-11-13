using System.Collections.Frozen;

namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 框架维护的文件类型白名单与文件头识别
/// </summary>
/// <remarks>
/// 一个文件要同时满足两条才算某种类型：扩展名对应这种类型，文件头也符合这种类型的签名。
/// 只改扩展名骗不过去，只改内容也骗不过去。业务不能自定义类型或签名，只能在上传档案里从这里选
/// </remarks>
public static class FileTypes
{
    /// <summary>
    /// 识别文件类型要读的开头字节数
    /// </summary>
    public const int HeaderLength = 4096;

    private static readonly byte[] _jpeg = [0xFF, 0xD8, 0xFF];
    private static readonly byte[] _png = [0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A];
    private static readonly byte[] _zip = [0x50, 0x4B, 0x03, 0x04];
    private static readonly byte[] _ole = [0xD0, 0xCF, 0x11, 0xE0, 0xA1, 0xB1, 0x1A, 0xE1];
    private static readonly FrozenDictionary<string, FileTypeDefinition> _byCode;
    private static readonly FrozenDictionary<string, FileTypeDefinition> _byExtension;

    static FileTypes()
    {
        FileTypeDefinition[] all =
        [
            new(FileType.Pdf, "PDF 文档", "application/pdf", [".pdf"], header => header.StartsWith("%PDF-"u8)),
            new(FileType.Jpeg, "JPEG 图片", "image/jpeg", [".jpg", ".jpeg"], header => header.StartsWith(_jpeg)),
            new(FileType.Png, "PNG 图片", "image/png", [".png"], header => header.StartsWith(_png)),
            new(FileType.Gif, "GIF 图片", "image/gif", [".gif"], header => header.StartsWith("GIF87a"u8) || header.StartsWith("GIF89a"u8)),
            new(FileType.Webp, "WebP 图片", "image/webp", [".webp"], IsWebp),
            new(FileType.Docx, "Word 文档", "application/vnd.openxmlformats-officedocument.wordprocessingml.document", [".docx"], header => IsOpenXml(header, "word/"u8)),
            new(FileType.Xlsx, "Excel 工作簿", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", [".xlsx"], header => IsOpenXml(header, "xl/"u8)),
            new(FileType.Pptx, "PowerPoint 演示文稿", "application/vnd.openxmlformats-officedocument.presentationml.presentation", [".pptx"], header => IsOpenXml(header, "ppt/"u8)),
            new(FileType.Doc, "Word 文档（旧版）", "application/msword", [".doc"], IsOle),
            new(FileType.Xls, "Excel 工作簿（旧版）", "application/vnd.ms-excel", [".xls"], IsOle),
            new(FileType.Ppt, "PowerPoint 演示文稿（旧版）", "application/vnd.ms-powerpoint", [".ppt"], IsOle),
            new(FileType.Txt, "纯文本", "text/plain", [".txt"], IsText),
            new(FileType.Csv, "CSV 文本", "text/csv", [".csv"], IsText),
            new(FileType.Zip, "ZIP 压缩包", "application/zip", [".zip"], IsZip),
        ];
        _byCode = all.ToFrozenDictionary(t => t.Code, StringComparer.Ordinal);
        _byExtension = all.SelectMany(t => t.Extensions.Select(e => (Extension: e, Type: t))).ToFrozenDictionary(x => x.Extension, x => x.Type, StringComparer.Ordinal);
        All = all;
    }

    /// <summary>
    /// 全部类型
    /// </summary>
    public static IReadOnlyList<FileTypeDefinition> All { get; }

    /// <summary>
    /// 按编码取类型，不认识时返回空
    /// </summary>
    /// <param name="code">编码，见 <see cref="FileType" /></param>
    public static FileTypeDefinition? Find(string code) => _byCode.GetValueOrDefault(code);

    /// <summary>
    /// 扩展名对应的类型，不认识时返回空
    /// </summary>
    /// <param name="fileName">文件名，只看最后一个扩展名，不分大小写</param>
    public static FileTypeDefinition? ByFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        var extension = Path.GetExtension(fileName.AsSpan()).ToString().ToLowerInvariant();
        return _byExtension.GetValueOrDefault(extension);
    }

    /// <summary>
    /// 识别一个文件：扩展名对应的类型，加上文件头符合这种类型
    /// </summary>
    /// <param name="fileName">文件名</param>
    /// <param name="header">文件开头的字节，最多 <see cref="HeaderLength" /> 个；空文件没有类型</param>
    /// <returns>识别出的类型；扩展名不认识、文件头不符、空文件时返回空</returns>
    public static FileTypeDefinition? Identify(string fileName, ReadOnlySpan<byte> header) =>
        header.IsEmpty || ByFileName(fileName) is not { } type || !type.MatchesHeader(header) ? null : type;

    private static bool IsWebp(ReadOnlySpan<byte> header) =>
        header.Length >= 12 && header.StartsWith("RIFF"u8) && header.Slice(8, 4).SequenceEqual("WEBP"u8);

    private static bool IsZip(ReadOnlySpan<byte> header) => header.StartsWith(_zip);

    // Office 2007 以后的格式都是 ZIP。扩展名决定它是哪一种，文件头里如果已经出现了别的 Office 类型的目录名，说明扩展名是改的
    private static bool IsOpenXml(ReadOnlySpan<byte> header, ReadOnlySpan<byte> own)
    {
        if (!IsZip(header))
        {
            return false;
        }

        return (own.SequenceEqual("word/"u8) || header.IndexOf("word/"u8) < 0)
            && (own.SequenceEqual("xl/"u8) || header.IndexOf("xl/"u8) < 0)
            && (own.SequenceEqual("ppt/"u8) || header.IndexOf("ppt/"u8) < 0);
    }

    // 复合文档，旧版 Word、Excel、PowerPoint 都是这个头；具体是哪一种在文件深处，这里只认头，由扩展名决定
    private static bool IsOle(ReadOnlySpan<byte> header) => header.StartsWith(_ole);

    // 文本没有签名：开头里不能有空字节和别的控制字符，否则是二进制。高位字节不限，UTF-8、GBK 的文本都有
    private static bool IsText(ReadOnlySpan<byte> header)
    {
        foreach (var value in header)
        {
            if (value is (< 0x20 and not (0x09 or 0x0A or 0x0C or 0x0D)) or 0x7F)
            {
                return false;
            }
        }

        return true;
    }
}

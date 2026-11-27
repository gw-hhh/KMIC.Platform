using System.Collections.Frozen;

namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>框架维护的文件类型白名单与文件头识别。</summary>
public static class FileTypes
{
    /// <summary>识别文件类型所需读取的文件头最大字节数。</summary>
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

    public static IReadOnlyList<FileTypeDefinition> All { get; }

    /// <summary>按编码查找文件类型。</summary>
    public static FileTypeDefinition? Find(string code) => _byCode.GetValueOrDefault(code);

    /// <summary>判断是否允许浏览器内联预览。</summary>
    public static bool OpensInBrowser(string code) => code is FileType.Pdf or FileType.Jpeg or FileType.Png or FileType.Gif or FileType.Webp;

    /// <summary>按扩展名查找文件类型。</summary>
    public static FileTypeDefinition? ByFileName(string fileName)
    {
        ArgumentNullException.ThrowIfNull(fileName);

        var extension = Path.GetExtension(fileName.AsSpan()).ToString().ToLowerInvariant();
        return _byExtension.GetValueOrDefault(extension);
    }

    /// <summary>结合扩展名与文件头（最多 <see cref="HeaderLength" /> 字节）识别文件类型。</summary>
    public static FileTypeDefinition? Identify(string fileName, ReadOnlySpan<byte> header) =>
        header.IsEmpty || ByFileName(fileName) is not { } type || !type.MatchesHeader(header) ? null : type;

    private static bool IsWebp(ReadOnlySpan<byte> header) =>
        header.Length >= 12 && header.StartsWith("RIFF"u8) && header.Slice(8, 4).SequenceEqual("WEBP"u8);

    private static bool IsZip(ReadOnlySpan<byte> header) => header.StartsWith(_zip);

    // Office 2007+ 格式本质为 ZIP，具体类型由扩展名判定；若文件头包含其他 Office 类型的目录特征，则说明扩展名被篡改
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

    // 复合文档（OLE）格式，旧版 Word/Excel/PowerPoint 共用此文件头；具体类型位于文件内部，此处仅校验文件头并结合扩展名判定
    private static bool IsOle(ReadOnlySpan<byte> header) => header.StartsWith(_ole);

    // 纯文本无固定魔数签名：头部不得包含空字节或非空白控制字符（否则视为二进制），高位字节不限以兼容 UTF-8 与 GBK 等编码
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

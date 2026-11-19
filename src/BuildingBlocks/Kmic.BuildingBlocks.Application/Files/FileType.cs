namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 框架认识的文件类型编码：上传档案的白名单、文件元数据里的类型都用这些编码
/// </summary>
public static class FileType
{
    /// <summary>
    /// PDF 文档
    /// </summary>
    public const string Pdf = "pdf";

    /// <summary>
    /// JPEG 图片
    /// </summary>
    public const string Jpeg = "jpeg";

    /// <summary>
    /// PNG 图片
    /// </summary>
    public const string Png = "png";

    /// <summary>
    /// GIF 图片
    /// </summary>
    public const string Gif = "gif";

    /// <summary>
    /// WebP 图片
    /// </summary>
    public const string Webp = "webp";

    /// <summary>
    /// Word 文档（docx）
    /// </summary>
    public const string Docx = "docx";

    /// <summary>
    /// Excel 工作簿（xlsx）
    /// </summary>
    public const string Xlsx = "xlsx";

    /// <summary>
    /// PowerPoint 演示文稿（pptx）
    /// </summary>
    public const string Pptx = "pptx";

    /// <summary>
    /// 旧版 Word 文档（doc）
    /// </summary>
    public const string Doc = "doc";

    /// <summary>
    /// 旧版 Excel 工作簿（xls）
    /// </summary>
    public const string Xls = "xls";

    /// <summary>
    /// 旧版 PowerPoint 演示文稿（ppt）
    /// </summary>
    public const string Ppt = "ppt";

    /// <summary>
    /// 纯文本
    /// </summary>
    public const string Txt = "txt";

    /// <summary>
    /// 逗号分隔文本
    /// </summary>
    public const string Csv = "csv";

    /// <summary>
    /// ZIP 压缩包
    /// </summary>
    public const string Zip = "zip";
}

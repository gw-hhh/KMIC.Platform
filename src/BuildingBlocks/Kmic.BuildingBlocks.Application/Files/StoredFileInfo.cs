namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 已经写进存储的上传文件：对象的键与版本、实际大小、SHA-256、识别出的类型
/// </summary>
/// <param name="Key">对象在存储里的键</param>
/// <param name="VersionId">对象版本 ID；存储没有开启版本控制时为空</param>
/// <param name="Size">实际字节数，不是客户端声明的</param>
/// <param name="Sha256">内容的 SHA-256，小写十六进制</param>
/// <param name="Type">识别出的文件类型</param>
public sealed record StoredFileInfo(string Key, string? VersionId, long Size, string Sha256, FileTypeDefinition Type);

using Kmic.BuildingBlocks.Application.Files;

namespace Kmic.BuildingBlocks.Infrastructure.Files;

/// <summary>
/// 已经写进存储的上传文件：对象、实际大小、SHA-256、识别出的类型
/// </summary>
/// <param name="Stored">存储里的对象；开启版本控制时带对象版本 ID</param>
/// <param name="Size">实际字节数，不是客户端声明的</param>
/// <param name="Sha256">内容的 SHA-256，小写十六进制</param>
/// <param name="Type">识别出的文件类型</param>
public sealed record StoredFileInfo(StoredObject Stored, long Size, string Sha256, FileTypeDefinition Type);

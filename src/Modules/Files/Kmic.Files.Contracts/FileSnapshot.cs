namespace Kmic.Files.Contracts;

/// <summary>
/// 关联时的文件信息，业务方保存后可直接用于列表与详情显示
/// </summary>
/// <param name="FileId">文件 ID</param>
/// <param name="Name">显示用的文件名</param>
/// <param name="Size">字节数</param>
/// <param name="FileType">文件类型编码</param>
public sealed record FileSnapshot(Guid FileId, string Name, long Size, string FileType);

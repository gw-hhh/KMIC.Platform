namespace Kmic.Files.Contracts;

/// <summary>
/// 关联时从 Files 取到的文件快照：业务的关联行保存它，列表与详情就不用再查 Files
/// </summary>
/// <param name="FileId">文件 ID</param>
/// <param name="Name">显示用的文件名</param>
/// <param name="Size">字节数</param>
/// <param name="FileType">文件类型编码</param>
public sealed record FileSnapshot(Guid FileId, string Name, long Size, string FileType);

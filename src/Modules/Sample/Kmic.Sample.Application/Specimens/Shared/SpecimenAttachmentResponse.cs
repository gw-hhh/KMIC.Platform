namespace Kmic.Sample.Application.Specimens.Shared;

/// <summary>示例附件信息，使用关联时保存的文件名、大小和类型快照</summary>
/// <param name="Id">附件 ID</param>
/// <param name="Name">文件名</param>
/// <param name="Size">字节数</param>
/// <param name="FileType">文件类型编码</param>
public sealed record SpecimenAttachmentResponse(Guid Id, string Name, long Size, string FileType);

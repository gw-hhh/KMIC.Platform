namespace Kmic.Sample.Application.Specimens.Shared;

/// <summary>
/// 标本附件，用关联时保存的快照显示，不查 Files
/// </summary>
/// <param name="Id">附件</param>
/// <param name="Name">文件名</param>
/// <param name="Size">字节数</param>
/// <param name="FileType">文件类型编码</param>
public sealed record SpecimenAttachmentResponse(Guid Id, string Name, long Size, string FileType);

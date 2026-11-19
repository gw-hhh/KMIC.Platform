using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>
/// 示例的附件
/// </summary>
public sealed class SpecimenAttachment : Entity<SpecimenAttachmentId>
{
    private SpecimenAttachment(SpecimenAttachmentId id, StoredFileId fileId, string fileName, long size, string fileType)
        : base(id)
    {
        FileId = fileId;
        FileName = fileName;
        Size = size;
        FileType = fileType;
    }

    /// <summary>
    /// 文件，Files 模块里的 ID
    /// </summary>
    public StoredFileId FileId { get; private set; }

    /// <summary>
    /// 关联时的文件名快照
    /// </summary>
    public string FileName { get; private set; }

    /// <summary>
    /// 关联时的字节数快照
    /// </summary>
    public long Size { get; private set; }

    /// <summary>
    /// 关联时的文件类型编码快照
    /// </summary>
    public string FileType { get; private set; }

    internal static SpecimenAttachment Create(StoredFileId fileId, string fileName, long size, string fileType) =>
        new(SpecimenAttachmentId.New(), fileId, fileName, size, fileType);
}

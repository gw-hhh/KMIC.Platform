using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>
/// 示例标本的附件
/// </summary>
/// <remarks>
/// 聚合内的子实体，演示业务模块怎样保存文件的关联：只存文件 ID 和关联时取到的快照（名称、大小、类型），
/// 列表与详情不用再查 Files。文件内容和生命周期归 Files，这里的行与 Files 里的引用登记是同一件事的两份记录
/// </remarks>
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

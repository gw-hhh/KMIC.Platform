using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 文件的状态
/// </summary>
public sealed class StoredFileStatus : Enumeration<StoredFileStatus>
{
    /// <summary>
    /// 上传中：元数据已登记，内容还在写入存储
    /// </summary>
    public static readonly StoredFileStatus Uploading = new("Uploading", "上传中");

    /// <summary>
    /// 扫描中：内容已写入，等扫描器给结论（没有扫描器时不经过这个状态）
    /// </summary>
    public static readonly StoredFileStatus Scanning = new("Scanning", "扫描中");

    /// <summary>
    /// 可用：临时文件，24 小时内没有被业务关联就会清理
    /// </summary>
    public static readonly StoredFileStatus Available = new("Available", "临时");

    /// <summary>
    /// 已关联：至少有一个业务对象引用它
    /// </summary>
    public static readonly StoredFileStatus Attached = new("Attached", "已关联");

    /// <summary>
    /// 已解除关联：没有引用了，保留期内可以重新关联，期满清理
    /// </summary>
    public static readonly StoredFileStatus Detached = new("Detached", "已解除关联");

    /// <summary>
    /// 被拒绝：类型或大小不符、扫描没通过、写入失败，不能关联，很快清理
    /// </summary>
    public static readonly StoredFileStatus Rejected = new("Rejected", "被拒绝");

    /// <summary>
    /// 删除中：已经在删除队列里，先删对象、再删行
    /// </summary>
    public static readonly StoredFileStatus Deleting = new("Deleting", "删除中");

    /// <summary>
    /// 已彻底删除：运维按“彻底删除”处理过，行保留作记录，引用方显示“文件已删除”，不能恢复
    /// </summary>
    public static readonly StoredFileStatus Purged = new("Purged", "已彻底删除");

    private StoredFileStatus(string code, string name)
        : base(code, name)
    {
    }
}

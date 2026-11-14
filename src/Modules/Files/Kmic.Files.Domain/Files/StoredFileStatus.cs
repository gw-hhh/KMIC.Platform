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
    /// 可用：未关联的上传文件或模块生成文件，达到各自保留期限后可清理
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
    /// 被拒绝：文件检查失败或存储对象丢失，不可关联，保留期满后可清理
    /// </summary>
    public static readonly StoredFileStatus Rejected = new("Rejected", "被拒绝");

    /// <summary>
    /// 删除中：已经在删除队列里，先删对象、再删行
    /// </summary>
    public static readonly StoredFileStatus Deleting = new("Deleting", "删除中");

    /// <summary>
    /// 已彻底删除：保留元数据和引用，禁止读取或恢复；存储对象由删除工作器处理
    /// </summary>
    public static readonly StoredFileStatus Purged = new("Purged", "已彻底删除");

    private StoredFileStatus(string code, string name)
        : base(code, name)
    {
    }
}

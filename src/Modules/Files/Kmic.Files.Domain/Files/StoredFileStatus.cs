using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Domain.Files;

public sealed class StoredFileStatus : Enumeration<StoredFileStatus>
{
    /// <summary>元数据已登记，内容正在写入存储</summary>
    public static readonly StoredFileStatus Uploading = new("Uploading", "上传中");

    /// <summary>等待内容安全扫描结果</summary>
    public static readonly StoredFileStatus Scanning = new("Scanning", "扫描中");

    /// <summary>未关联的上传或生成文件，保留期满后可清理</summary>
    public static readonly StoredFileStatus Available = new("Available", "待关联");

    /// <summary>至少被一个业务对象引用</summary>
    public static readonly StoredFileStatus Attached = new("Attached", "已关联");

    /// <summary>已解除关联，保留期内可重新关联，期满后清理</summary>
    public static readonly StoredFileStatus Detached = new("Detached", "已解除关联");

    /// <summary>检查失败或存储对象丢失，不可关联，保留期满后可清理</summary>
    public static readonly StoredFileStatus Rejected = new("Rejected", "被拒绝");

    /// <summary>已进入删除队列，先删除存储对象再删除记录</summary>
    public static readonly StoredFileStatus Deleting = new("Deleting", "删除中");

    /// <summary>保留元数据和引用，禁止读取或恢复；存储对象由删除队列异步清理</summary>
    public static readonly StoredFileStatus Purged = new("Purged", "已彻底删除");

    private StoredFileStatus(string code, string name)
        : base(code, name)
    {
    }
}

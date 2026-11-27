using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Domain.Files;

public sealed class StoredFileAction : Enumeration<StoredFileAction>
{
    /// <summary>写入完成，无需扫描</summary>
    public static readonly StoredFileAction Complete = new("Complete", "上传完成");

    /// <summary>写入完成，待内容扫描</summary>
    public static readonly StoredFileAction BeginScan = new("BeginScan", "开始扫描");

    public static readonly StoredFileAction PassScan = new("PassScan", "扫描通过");

    /// <summary>拒绝上传或扫描中的文件</summary>
    public static readonly StoredFileAction Reject = new("Reject", "拒绝");

    public static readonly StoredFileAction Attach = new("Attach", "关联");

    /// <summary>最后一个引用解除</summary>
    public static readonly StoredFileAction Detach = new("Detach", "解除关联");

    /// <summary>到期，进入删除队列</summary>
    public static readonly StoredFileAction Expire = new("Expire", "到期删除");

    public static readonly StoredFileAction Purge = new("Purge", "彻底删除");

    /// <summary>对账发现对象存储中已无对应内容</summary>
    public static readonly StoredFileAction Lose = new("Lose", "对象丢失");

    private StoredFileAction(string code, string name)
        : base(code, name)
    {
    }
}

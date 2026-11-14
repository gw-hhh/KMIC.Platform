using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Files.Domain.Files;

/// <summary>
/// 文件状态机的动作
/// </summary>
public sealed class StoredFileAction : Enumeration<StoredFileAction>
{
    /// <summary>
    /// 写入完成，没有扫描器
    /// </summary>
    public static readonly StoredFileAction Complete = new("Complete", "上传完成");

    /// <summary>
    /// 写入完成，交给扫描器
    /// </summary>
    public static readonly StoredFileAction BeginScan = new("BeginScan", "开始扫描");

    /// <summary>
    /// 扫描通过
    /// </summary>
    public static readonly StoredFileAction PassScan = new("PassScan", "扫描通过");

    /// <summary>
    /// 拒绝：类型或大小不符、扫描没通过、写入失败
    /// </summary>
    public static readonly StoredFileAction Reject = new("Reject", "拒绝");

    /// <summary>
    /// 被业务对象关联
    /// </summary>
    public static readonly StoredFileAction Attach = new("Attach", "关联");

    /// <summary>
    /// 最后一个引用解除
    /// </summary>
    public static readonly StoredFileAction Detach = new("Detach", "解除关联");

    /// <summary>
    /// 到期，进入删除队列
    /// </summary>
    public static readonly StoredFileAction Expire = new("Expire", "到期删除");

    /// <summary>
    /// 彻底删除
    /// </summary>
    public static readonly StoredFileAction Purge = new("Purge", "彻底删除");

    /// <summary>
    /// 对象丢失：对账发现存储里已经没有它的内容
    /// </summary>
    public static readonly StoredFileAction Lose = new("Lose", "对象丢失");

    private StoredFileAction(string code, string name)
        : base(code, name)
    {
    }
}

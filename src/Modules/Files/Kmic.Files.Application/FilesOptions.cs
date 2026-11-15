using System.ComponentModel.DataAnnotations;

namespace Kmic.Files.Application;

/// <summary>
/// Files 模块的部署配置
/// </summary>
public sealed class FilesOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Files";

    /// <summary>
    /// 未关联临时文件的保留时长（小时），到期后可清理
    /// </summary>
    [Range(1, 24 * 14)]
    public int TempExpiryHours { get; init; } = 24;

    /// <summary>
    /// 解除关联后的保留天数，期间可重新关联，到期后可清理
    /// </summary>
    [Range(0, 3650)]
    public int DetachedRetentionDays { get; init; } = 30;

    /// <summary>
    /// 被拒绝文件的保留天数
    /// </summary>
    [Range(0, 365)]
    public int RejectedRetentionDays { get; init; } = 1;

    /// <summary>
    /// 单次关联的文件数上限，用于限制事务持锁数量
    /// </summary>
    [Range(1, 500)]
    public int MaxFilesPerCommand { get; init; } = 50;

    /// <summary>
    /// 关联共享锁的等待上限（秒）
    /// </summary>
    [Range(1, 60)]
    public int AttachLockTimeoutSeconds { get; init; } = 5;

    /// <summary>
    /// 单批清理的文件数上限，用于限制事务持锁数量；剩余文件留待下轮处理
    /// </summary>
    [Range(1, 5000)]
    public int PurgeBatchSize { get; init; } = 1000;

    /// <summary>
    /// 上传超时时长（分钟），达到后仍未完成则视为中断
    /// </summary>
    [Range(5, 24 * 60)]
    public int StaleUploadMinutes { get; init; } = 60;

    /// <summary>
    /// 对账的最短等待天数，避免处理仍在写入或等待登记的对象
    /// </summary>
    [Range(1, 30)]
    public int ReconcileMinAgeDays { get; init; } = 2;

    /// <summary>
    /// 单批对账数量，用于限制查询和消息探查规模
    /// </summary>
    [Range(10, 2000)]
    public int ReconcileBatchSize { get; init; } = 500;

    /// <summary>
    /// 单次对账的差异数上限，多余对象和缺失对象分别计数。
    /// 任一类超过上限则放弃本次对账，避免在存储配置错误或数据库还原后大批处理文件
    /// </summary>
    [Range(1, 100000)]
    public int ReconcileMaxChanges { get; init; } = 200;

    /// <summary>
    /// 模块生成文件的大小上限（MB），超过时写入失败
    /// </summary>
    [Range(1, 2048)]
    public int GeneratedFileMaxMb { get; init; } = 200;

    public TimeSpan TempExpiry => TimeSpan.FromHours(TempExpiryHours);

    public TimeSpan DetachedRetention => TimeSpan.FromDays(DetachedRetentionDays);

    public TimeSpan RejectedRetention => TimeSpan.FromDays(RejectedRetentionDays);

    public TimeSpan StaleUpload => TimeSpan.FromMinutes(StaleUploadMinutes);

    public TimeSpan ReconcileMinAge => TimeSpan.FromDays(ReconcileMinAgeDays);
}

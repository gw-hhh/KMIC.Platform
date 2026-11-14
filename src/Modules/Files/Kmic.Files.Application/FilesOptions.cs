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
    /// 上传完成后没有被关联的临时文件保留多少小时，过了成为清理候选
    /// </summary>
    [Range(1, 24 * 14)]
    public int TempExpiryHours { get; init; } = 24;

    /// <summary>
    /// 解除关联后的文件保留多少天：期内可以重新关联，期满清理
    /// </summary>
    [Range(0, 3650)]
    public int DetachedRetentionDays { get; init; } = 30;

    /// <summary>
    /// 被拒绝的文件保留多少天就清理
    /// </summary>
    [Range(0, 365)]
    public int RejectedRetentionDays { get; init; } = 1;

    /// <summary>
    /// 一次关联最多多少个文件：限制单个事务持有的锁数量
    /// </summary>
    [Range(1, 500)]
    public int MaxFilesPerCommand { get; init; } = 50;

    /// <summary>
    /// 关联时取共享锁最多等几秒
    /// </summary>
    [Range(1, 60)]
    public int AttachLockTimeoutSeconds { get; init; } = 5;

    /// <summary>
    /// 每次清理最多处理多少个到期的文件，剩下的下一轮再处理：限制单个事务持有的锁数量
    /// </summary>
    [Range(1, 5000)]
    public int PurgeBatchSize { get; init; } = 1000;

    /// <summary>
    /// 上传开始之后超过多少分钟还没有结果，就当作中断
    /// </summary>
    [Range(5, 24 * 60)]
    public int StaleUploadMinutes { get; init; } = 60;

    /// <summary>
    /// 对账只看存在了至少多少天的对象和文件：更新的可能还在写入或等待登记
    /// </summary>
    [Range(1, 30)]
    public int ReconcileMinAgeDays { get; init; } = 2;

    /// <summary>
    /// 对账每次处理多少个，限制单次查询和探查的规模
    /// </summary>
    [Range(10, 2000)]
    public int ReconcileBatchSize { get; init; } = 500;

    /// <summary>
    /// 对账一次最多处理多少个差异：存储里多出的对象、库里缺对象的文件各算一类。
    /// 超过就整个放弃，因为这更像是存储配置不对，或者数据库被还原过，而不是个别文件出了问题
    /// </summary>
    [Range(1, 100000)]
    public int ReconcileMaxChanges { get; init; } = 200;

    /// <summary>
    /// 模块生成的单个文件最大多少 MB，超过的写入失败
    /// </summary>
    [Range(1, 2048)]
    public int GeneratedFileMaxMb { get; init; } = 200;

    /// <summary>
    /// 临时文件保留多久
    /// </summary>
    public TimeSpan TempExpiry => TimeSpan.FromHours(TempExpiryHours);

    /// <summary>
    /// 解除关联后保留多久
    /// </summary>
    public TimeSpan DetachedRetention => TimeSpan.FromDays(DetachedRetentionDays);

    /// <summary>
    /// 被拒绝的文件保留多久
    /// </summary>
    public TimeSpan RejectedRetention => TimeSpan.FromDays(RejectedRetentionDays);

    /// <summary>
    /// 上传多久没有结果算中断
    /// </summary>
    public TimeSpan StaleUpload => TimeSpan.FromMinutes(StaleUploadMinutes);

    /// <summary>
    /// 对账的最小年龄
    /// </summary>
    public TimeSpan ReconcileMinAge => TimeSpan.FromDays(ReconcileMinAgeDays);
}

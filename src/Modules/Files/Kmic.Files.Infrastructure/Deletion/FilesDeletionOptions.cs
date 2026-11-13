using System.ComponentModel.DataAnnotations;

namespace Kmic.Files.Infrastructure.Deletion;

/// <summary>
/// 删除工作器的部署配置
/// </summary>
public sealed class FilesDeletionOptions
{
    /// <summary>
    /// 配置节
    /// </summary>
    public const string Section = "Files:Deletion";

    /// <summary>
    /// 同时删除几个文件
    /// </summary>
    [Range(1, 32)]
    public int Parallelism { get; init; } = 4;

    /// <summary>
    /// 一个文件最多尝试几次：用完还没删掉的留在队列里等人工处理，不再自动领取
    /// </summary>
    [Range(1, 100)]
    public int MaxAttempts { get; init; } = 8;

    /// <summary>
    /// 领取之后多少分钟没有结果，别的实例可以接管
    /// </summary>
    [Range(1, 120)]
    public int LeaseMinutes { get; init; } = 5;

    /// <summary>
    /// 没有被唤醒时，隔多少秒查一次队列
    /// </summary>
    [Range(1, 600)]
    public int PollSeconds { get; init; } = 10;

    /// <summary>
    /// 领取的租期
    /// </summary>
    public TimeSpan Lease => TimeSpan.FromMinutes(LeaseMinutes);

    /// <summary>
    /// 轮询间隔
    /// </summary>
    public TimeSpan Poll => TimeSpan.FromSeconds(PollSeconds);
}

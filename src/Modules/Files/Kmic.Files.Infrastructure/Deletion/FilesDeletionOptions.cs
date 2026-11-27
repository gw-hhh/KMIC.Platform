using System.ComponentModel.DataAnnotations;

namespace Kmic.Files.Infrastructure.Deletion;

/// <summary>文件删除队列部署配置</summary>
public sealed class FilesDeletionOptions
{
    public const string Section = "Files:Deletion";

    /// <summary>并行删除的文件数</summary>
    [Range(1, 32)]
    public int Parallelism { get; init; } = 4;

    /// <summary>单个文件的删除尝试上限，达到后停止自动领取，保留在队列中等待人工处理</summary>
    [Range(1, 100)]
    public int MaxAttempts { get; init; } = 8;

    /// <summary>占用时长（分钟），到期后其他实例可接管</summary>
    [Range(1, 120)]
    public int LeaseMinutes { get; init; } = 5;

    /// <summary>未收到唤醒时的队列轮询间隔（秒）</summary>
    [Range(1, 600)]
    public int PollSeconds { get; init; } = 10;

    public TimeSpan Lease => TimeSpan.FromMinutes(LeaseMinutes);

    public TimeSpan Poll => TimeSpan.FromSeconds(PollSeconds);
}

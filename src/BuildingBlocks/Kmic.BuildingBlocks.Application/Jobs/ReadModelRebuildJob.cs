namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 重建一个订阅的读模型
/// </summary>
/// <remarks>
/// 框架自己的作业，存在订阅方模块的作业表里，处理器由基础设施层注册
/// </remarks>
/// <param name="Subscription">订阅名</param>
/// <param name="RunId">重建轮次，在订阅方模块的轮次记录里</param>
/// <param name="ConfirmMissing">运维确认过源端找不到的行：只做收尾，不再受“找不到的行太多”的限制</param>
/// <param name="Scanned">确认收尾时，前一次作业已经扫描的行数</param>
[BackgroundJob("BuildingBlocks.RebuildReadModel", SystemOnly = true, Resumable = true, MaxConcurrentPerUser = 1)]
internal sealed record ReadModelRebuildJob(string Subscription, Guid RunId, bool ConfirmMissing = false, long Scanned = 0) : IBackgroundJob;

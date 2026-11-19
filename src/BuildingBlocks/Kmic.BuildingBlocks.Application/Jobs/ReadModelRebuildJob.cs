namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 重建一个订阅的读模型
/// </summary>
/// <param name="Subscription">订阅名</param>
/// <param name="RunId">重建轮次</param>
/// <param name="ConfirmMissing">运维确认过源端找不到的行</param>
/// <param name="Scanned">确认收尾时</param>
[BackgroundJob("BuildingBlocks.RebuildReadModel", "重建读模型", SystemOnly = true, Resumable = true, MaxConcurrentPerUser = 1)]
internal sealed record ReadModelRebuildJob(string Subscription, Guid RunId, bool ConfirmMissing = false, long Scanned = 0) : IBackgroundJob;

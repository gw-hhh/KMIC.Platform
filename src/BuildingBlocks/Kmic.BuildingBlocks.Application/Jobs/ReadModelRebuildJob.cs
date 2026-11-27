namespace Kmic.BuildingBlocks.Application.Jobs;

[BackgroundJob("BuildingBlocks.RebuildReadModel", "重建读模型", SystemOnly = true, Resumable = true, MaxConcurrentPerUser = 1)]
internal sealed record ReadModelRebuildJob(string Subscription, Guid RunId, bool ConfirmMissing = false, long Scanned = 0) : IBackgroundJob;

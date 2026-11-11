namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 重建一个订阅的读模型
/// </summary>
/// <remarks>
/// 框架自己的作业，存在订阅方模块的作业表里，处理器由基础设施层注册
/// </remarks>
/// <param name="Subscription">订阅名</param>
/// <param name="RunId">重建轮次，在订阅方模块的轮次记录里</param>
[BackgroundJob("BuildingBlocks.RebuildReadModel", SystemOnly = true, Resumable = true, MaxConcurrentPerUser = 1)]
internal sealed record ReadModelRebuildJob(string Subscription, Guid RunId) : IBackgroundJob;

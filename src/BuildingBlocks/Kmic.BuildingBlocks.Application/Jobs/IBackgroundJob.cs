namespace Kmic.BuildingBlocks.Application.Jobs;

/// <summary>
/// 后台作业的参数
/// </summary>
/// <remarks>
/// 用 sealed record 实现并标 [BackgroundJob]，同一模块里要有一个对应的 IJobHandler
/// </remarks>
public interface IBackgroundJob;

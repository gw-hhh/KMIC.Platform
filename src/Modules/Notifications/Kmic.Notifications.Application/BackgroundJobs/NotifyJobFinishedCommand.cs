using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.BuildingBlocks.Contracts.Jobs;

namespace Kmic.Notifications.Application.BackgroundJobs;

/// <summary>后台作业结束后通知提交人（自行取消的除外）</summary>
/// <param name="Message">作业结束事件</param>
/// <param name="SourceMessageId">来源消息的 ID</param>
/// <param name="SourceModule">作业所在模块</param>
[SystemOnly]
[NotAudited("发送请求和站内消息已有专门记录")]
public sealed record NotifyJobFinishedCommand(BackgroundJobFinished Message, Guid SourceMessageId, string SourceModule) : ICommand;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.Jobs;
using Kmic.BuildingBlocks.Contracts.Notifications;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Application.Processing;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.BackgroundJobs;

/// <summary>
/// 给作业提交人发站内消息
/// </summary>
public sealed class NotifyJobFinishedCommandHandler(NotificationProcessor processor) : ICommandHandler<NotifyJobFinishedCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(NotifyJobFinishedCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var message = command.Message;

        // 提交人自己取消的作业不再通知
        if (message.Result == BackgroundJobResult.Cancelled && message.CancelRequestedBy == message.SubmittedBy)
        {
            return Result.Success();
        }

        var notice = new BackgroundJobFinishedNotice(
            message.JobName, ResultText(message.Result), message.Result == BackgroundJobResult.Failed ? message.ErrorMessage ?? "" : "");
        return await processor.ProcessAsync(
            new NotificationRequest(
                BackgroundJobFinishedNotice.TemplateKey,
                [Recipient.OfUser(message.SubmittedBy)],
                NotificationModelJson.Serialize(notice),
                $"Framework.BackgroundJobFinished:{message.JobId}",
                Secret: false,
                HighPriority: false,
                command.SourceMessageId,
                command.SourceModule,
                InitiatorUserId: null),
            cancellationToken);
    }

    private static string ResultText(BackgroundJobResult result) => result switch
    {
        BackgroundJobResult.Succeeded => "已完成",
        BackgroundJobResult.Cancelled => "已取消",
        _ => "失败",
    };
}

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 处理 <see cref="ProcessSendNotificationCommand"/>
/// </summary>
public sealed class ProcessSendNotificationCommandHandler(NotificationProcessor processor) : ICommandHandler<ProcessSendNotificationCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(ProcessSendNotificationCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var message = command.Message;
        return await processor.ProcessAsync(
            new NotificationRequest(
                message.TemplateKey, message.To, message.Model, message.DedupKey, Secret: false, message.Priority == NotificationPriority.High, command.SourceMessageId, command.SourceModule, command.InitiatorUserId, message.AttachmentFileIds),
            cancellationToken);
    }
}

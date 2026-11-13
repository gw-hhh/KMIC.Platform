using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Application.Processing;

/// <summary>
/// 处理 <see cref="ProcessSendSecretNotificationCommand"/>
/// </summary>
public sealed class ProcessSendSecretNotificationCommandHandler(NotificationProcessor processor) : ICommandHandler<ProcessSendSecretNotificationCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(ProcessSendSecretNotificationCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var message = command.Message;
        return await processor.ProcessAsync(
            new NotificationRequest(message.TemplateKey, message.To, message.Model, message.DedupKey, Secret: true, HighPriority: true, command.SourceMessageId, command.SourceModule, command.InitiatorUserId),
            cancellationToken);
    }
}

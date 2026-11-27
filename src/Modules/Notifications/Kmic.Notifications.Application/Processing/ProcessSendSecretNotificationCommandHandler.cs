using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Notifications.Application.Processing;

public sealed class ProcessSendSecretNotificationCommandHandler(NotificationProcessor processor) : ICommandHandler<ProcessSendSecretNotificationCommand>
{
    public async ValueTask<Result> Handle(ProcessSendSecretNotificationCommand command, CancellationToken cancellationToken)
    {
        var message = command.Message;
        return await processor.ProcessAsync(
            new NotificationRequest(message.TemplateKey, message.To, message.Model, message.DedupKey, Secret: true, HighPriority: true, command.SourceMessageId, command.SourceModule, command.InitiatorUserId),
            cancellationToken);
    }
}

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Notifications.Contracts;

namespace Kmic.Notifications.Application.Processing;

public sealed class ProcessSendNotificationCommandHandler(NotificationProcessor processor) : ICommandHandler<ProcessSendNotificationCommand>
{
    public async ValueTask<Result> Handle(ProcessSendNotificationCommand command, CancellationToken cancellationToken)
    {
        var message = command.Message;
        return await processor.ProcessAsync(
            new NotificationRequest(
                message.TemplateKey, message.To, message.Model, message.DedupKey, Secret: false, message.Priority == NotificationPriority.High, command.SourceMessageId, command.SourceModule, command.InitiatorUserId, message.AttachmentFileIds),
            cancellationToken);
    }
}

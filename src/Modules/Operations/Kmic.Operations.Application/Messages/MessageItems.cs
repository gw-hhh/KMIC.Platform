using System.Text.Json;
using Kmic.BuildingBlocks.Application.Integration;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages;

internal static class MessageItems
{
    public static MessageBoxKind ToKind(MessageBox box) => box == MessageBox.Inbox ? MessageBoxKind.Inbox : MessageBoxKind.Outbox;

    public static MessageBox ToBox(MessageBoxKind kind) => kind == MessageBoxKind.Inbox ? MessageBox.Inbox : MessageBox.Outbox;

    public static DeadMessageItem ToItem(DeadMessage message) =>
        new(
            message.Id,
            ToKind(message.Box),
            message.Module,
            message.Subscription,
            message.MessageId,
            message.MessageType,
            message.SchemaVersion,
            message.OccurredAt,
            message.Attempts,
            message.LastError);

    public static JsonElement ElementOf(string json)
    {
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }
}

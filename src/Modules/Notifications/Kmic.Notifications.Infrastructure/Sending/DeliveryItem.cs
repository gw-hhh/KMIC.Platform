namespace Kmic.Notifications.Infrastructure.Sending;

internal sealed record DeliveryItem(
    Guid Id,
    string Channel,
    Guid? RecipientUserId,
    string? Address,
    string Role,
    string Subject,
    string? Body,
    string? SmsParameters,
    string? AttachmentFileIds,
    string TemplateKey,
    int Attempts,
    DateTimeOffset CreatedAt);

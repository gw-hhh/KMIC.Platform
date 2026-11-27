namespace Kmic.Api.Tests.Notifications;

public sealed record ReceivedMail(string From, IReadOnlyList<string> Recipients, string Data);

namespace Spike16.Tests;

internal sealed record ClaimedInbox(Guid Id, Guid MessageId, Guid Token);

namespace Kmic.BuildingBlocks.Application.Access;

public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

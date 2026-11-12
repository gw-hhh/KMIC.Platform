namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 签发出的访问令牌
/// </summary>
/// <param name="Value">令牌本身</param>
/// <param name="ExpiresAt">过期时间</param>
public sealed record AccessToken(string Value, DateTimeOffset ExpiresAt);

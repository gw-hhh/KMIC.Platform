namespace Kmic.BuildingBlocks.Application.Authorization;

/// <param name="RenamedFrom">更名前的旧编码，未更名时为 null。</param>
public sealed record PermissionDefinition(string Code, string Module, string Group, string DisplayName, bool Sensitive, string? RenamedFrom = null);

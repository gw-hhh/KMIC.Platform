namespace Kmic.BuildingBlocks.Application.Files;

/// <param name="VersionId">对象版本 ID；存储未开启版本控制时为 null。</param>
public sealed record StoredFileInfo(string Key, string? VersionId, long Size, string Sha256, FileTypeDefinition Type);

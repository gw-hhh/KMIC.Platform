namespace Kmic.BuildingBlocks.Application.Files;

public sealed record FileTypeDefinition(string Code, string DisplayName, string ContentType, IReadOnlyList<string> Extensions, FileHeaderCheck MatchesHeader);

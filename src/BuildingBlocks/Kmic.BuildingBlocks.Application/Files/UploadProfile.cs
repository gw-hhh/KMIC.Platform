namespace Kmic.BuildingBlocks.Application.Files;

public sealed record UploadProfile(string Name, string DisplayName, string Module, long MaxBytes, IReadOnlyList<string> AllowedTypes, string Permission)
{
    public bool Allows(string code) => AllowedTypes.Contains(code, StringComparer.Ordinal);
}

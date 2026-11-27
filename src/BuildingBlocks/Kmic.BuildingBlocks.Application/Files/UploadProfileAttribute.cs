namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>声明上传配置（大小上限、允许的文件类型及所需权限点，应选用对应业务的编辑类权限）。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class UploadProfileAttribute(string name, string displayName) : Attribute
{
    /// <summary>单文件大小绝对上限，各上传配置不可超过此值。</summary>
    public const long AbsoluteMaxBytes = 2L * 1024 * 1024 * 1024;

    public string Name { get; } = name;

    public string DisplayName { get; } = displayName;

    public long MaxBytes { get; init; }

    /// <summary>允许的文件类型编码，取值见 <see cref="FileType" />。</summary>
    public string[] AllowedTypes { get; init; } = [];

    public string Permission { get; init; } = string.Empty;
}

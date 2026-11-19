namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 声明上传档案：大小上限、允许的类型和上传权限
/// </summary>
/// <remarks>
/// 权限点用业务的“编辑”类权限
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
public sealed class UploadProfileAttribute(string name, string displayName) : Attribute
{
    /// <summary>
    /// 任何档案的单文件上限都不能超过它，防止一个档案把上限开到离谱
    /// </summary>
    public const long AbsoluteMaxBytes = 2L * 1024 * 1024 * 1024;

    /// <summary>
    /// 档案名
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// 显示名
    /// </summary>
    public string DisplayName { get; } = displayName;

    /// <summary>
    /// 单个文件最大字节数
    /// </summary>
    public long MaxBytes { get; init; }

    /// <summary>
    /// 允许的文件类型，取 <see cref="FileType" /> 里的编码
    /// </summary>
    public string[] AllowedTypes { get; init; } = [];

    /// <summary>
    /// 上传要有的权限点
    /// </summary>
    public string Permission { get; init; } = string.Empty;
}

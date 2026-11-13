namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 声明一个上传档案：这一类附件最大多大、允许哪些类型、要什么权限才能上传
/// </summary>
/// <remarks>
/// 写在模块 Application 里的一个静态类上，一个类可以声明几个档案。上传本身就要求有编辑该类业务对象的能力，
/// 所以权限点用业务的“编辑”类权限，避免任何登录用户都能往存储里塞文件
/// </remarks>
/// <param name="name">档案名，以模块名加点开头，全局唯一，如 Sample.ContractScan</param>
/// <param name="displayName">显示名</param>
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

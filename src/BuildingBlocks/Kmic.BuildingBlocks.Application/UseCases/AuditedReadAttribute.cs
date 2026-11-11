namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 记录读取审计
/// </summary>
/// <remarks>
/// 需要敏感权限点的查询必须加
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AuditedReadAttribute : Attribute;

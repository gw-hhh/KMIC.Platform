namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 查询成功后写审计事件；需要敏感权限点的查询必须使用
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AuditedReadAttribute : Attribute;

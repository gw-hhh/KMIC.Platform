namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>记录读取审计，涉及敏感权限点的查询须标注此特性。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AuditedReadAttribute : Attribute;

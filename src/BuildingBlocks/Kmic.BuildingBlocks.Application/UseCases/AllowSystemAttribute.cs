namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>允许系统身份执行（仅可与 [RequirePermission] 或 [AllowAuthenticated] 组合使用）。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowSystemAttribute : Attribute;

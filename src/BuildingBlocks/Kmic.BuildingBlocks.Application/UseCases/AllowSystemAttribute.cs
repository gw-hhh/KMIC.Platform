namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 允许系统身份执行
/// </summary>
/// <remarks>
/// 只能和 [RequirePermission] 或 [AllowAuthenticated] 一起用
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowSystemAttribute : Attribute;

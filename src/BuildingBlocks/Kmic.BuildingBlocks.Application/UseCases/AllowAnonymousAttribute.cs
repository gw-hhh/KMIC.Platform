namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 允许匿名访问
/// </summary>
/// <remarks>
/// 只有 Identity 模块能用
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowAnonymousAttribute : Attribute;

namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 登录即可访问
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowAuthenticatedAttribute : Attribute;

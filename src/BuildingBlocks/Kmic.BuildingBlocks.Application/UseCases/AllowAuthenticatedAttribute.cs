namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 任何已登录用户都可以执行，如读取自己的偏好
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowAuthenticatedAttribute : Attribute;

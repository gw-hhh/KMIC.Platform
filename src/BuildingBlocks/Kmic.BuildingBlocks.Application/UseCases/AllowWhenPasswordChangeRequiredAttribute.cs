namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 需要改密码时仍可执行
/// </summary>
/// <remarks>
/// 只能和 [AllowAuthenticated] 一起用，如修改密码、退出登录
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowWhenPasswordChangeRequiredAttribute : Attribute;

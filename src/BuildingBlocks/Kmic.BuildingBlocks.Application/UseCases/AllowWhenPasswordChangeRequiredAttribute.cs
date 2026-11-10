namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 用户必须先改密码时仍可执行（修改密码、me、退出）；只能与 [AllowAuthenticated] 叠加
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowWhenPasswordChangeRequiredAttribute : Attribute;

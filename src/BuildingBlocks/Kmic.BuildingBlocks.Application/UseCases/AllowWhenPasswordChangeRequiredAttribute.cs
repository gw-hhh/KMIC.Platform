namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>允许在强制修改密码状态下执行（如修改密码、退出登录），仅可与 [AllowAuthenticated] 组合使用。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowWhenPasswordChangeRequiredAttribute : Attribute;

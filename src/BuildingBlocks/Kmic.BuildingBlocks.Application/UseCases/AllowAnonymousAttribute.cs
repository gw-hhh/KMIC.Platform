namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 未登录也可以执行（登录、刷新、发送验证码、重置密码）；只允许 Identity 模块使用（架构测试）
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowAnonymousAttribute : Attribute;

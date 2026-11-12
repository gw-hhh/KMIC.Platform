namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 只读的模拟会话里仍可执行
/// </summary>
/// <remarks>
/// 模拟身份默认只读，授权行为拒绝其中的所有命令。但结束模拟、退出登录这类命令不修改业务数据，
/// 被拒绝反而让人结束不了模拟会话，所以用它声明例外。只能和 [AllowAuthenticated] 一起用，只放在不改业务数据的命令上
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowWhenImpersonationReadOnlyAttribute : Attribute;

namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 只读的模拟会话里仍可执行
/// </summary>
/// <remarks>
/// 只放在不改业务数据的命令上
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowWhenImpersonationReadOnlyAttribute : Attribute;

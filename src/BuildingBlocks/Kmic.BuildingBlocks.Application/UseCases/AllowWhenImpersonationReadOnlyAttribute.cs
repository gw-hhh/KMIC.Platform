namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>允许在只读模拟会话中执行，仅可用于不修改业务数据的命令。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowWhenImpersonationReadOnlyAttribute : Attribute;

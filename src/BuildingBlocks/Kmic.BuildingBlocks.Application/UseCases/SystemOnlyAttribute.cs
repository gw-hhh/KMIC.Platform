namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 只允许系统身份执行
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SystemOnlyAttribute : Attribute;

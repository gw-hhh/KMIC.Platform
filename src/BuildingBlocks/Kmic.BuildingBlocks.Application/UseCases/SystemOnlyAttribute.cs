namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 只能以系统身份执行，HTTP 调用一律拒绝
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class SystemOnlyAttribute : Attribute;

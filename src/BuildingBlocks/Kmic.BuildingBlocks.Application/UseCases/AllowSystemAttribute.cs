namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 也允许以系统身份执行（定时任务、事件处理）；只能与 [RequirePermission]、[AllowAuthenticated] 叠加
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class AllowSystemAttribute : Attribute;

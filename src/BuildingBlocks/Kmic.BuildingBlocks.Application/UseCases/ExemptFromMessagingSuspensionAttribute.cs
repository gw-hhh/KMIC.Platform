namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>消息容量软线熔断期间仍可放行的命令；硬线熔断时仍拒绝执行。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class ExemptFromMessagingSuspensionAttribute : Attribute;

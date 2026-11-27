namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>启用幂等校验，HTTP 请求须携带 Idempotency-Key 请求头。</summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IdempotentAttribute : Attribute;

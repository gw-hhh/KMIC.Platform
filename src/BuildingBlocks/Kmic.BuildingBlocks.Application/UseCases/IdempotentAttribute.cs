namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 需要幂等键：HTTP 调用必须带 Idempotency-Key，同一个键的重复请求只执行一次
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IdempotentAttribute : Attribute;

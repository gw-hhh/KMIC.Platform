namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 需要幂等键
/// </summary>
/// <remarks>
/// HTTP 请求必须带 Idempotency-Key，同一个键的重复请求只执行一次
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IdempotentAttribute : Attribute;

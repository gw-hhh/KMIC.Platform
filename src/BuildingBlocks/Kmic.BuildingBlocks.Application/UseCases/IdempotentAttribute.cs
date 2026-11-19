namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 需要幂等键：HTTP 请求必须带 Idempotency-Key
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class IdempotentAttribute : Attribute;

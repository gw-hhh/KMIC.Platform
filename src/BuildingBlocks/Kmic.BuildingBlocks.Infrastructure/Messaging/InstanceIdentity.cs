namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 本进程的身份：每次启动换一个 ID，心跳按它登记
/// </summary>
/// <param name="Id">进程的 ID</param>
/// <param name="Host">机器名</param>
/// <param name="Roles">本实例的角色</param>
internal sealed record InstanceIdentity(Guid Id, string Host, string Roles);

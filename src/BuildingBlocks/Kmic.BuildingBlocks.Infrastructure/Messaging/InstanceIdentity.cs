namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 当前进程实例标识（每次启动生成唯一 ID 用于心跳登记）
internal sealed record InstanceIdentity(Guid Id, string Host, string Roles);

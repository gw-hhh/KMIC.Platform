namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

// 当前作用域正在处理的 Inbox 消息（作为派生消息的 CausationId）
internal sealed class CurrentInboxMessage
{
    public Guid? MessageId { get; set; }

    // 提交前若检测到占用已被其他处理者接管，回滚整个命令事务
    public bool TakenOver { get; set; }
}

namespace Kmic.BuildingBlocks.Infrastructure.Messaging;

/// <summary>
/// 本作用域正在处理的 Inbox 消息，用作新消息的 CausationId
/// </summary>
internal sealed class CurrentInboxMessage
{
    public Guid? MessageId { get; set; }

    // 提交前发现占用已被别的处理者接管，命令整体回滚
    public bool TakenOver { get; set; }
}

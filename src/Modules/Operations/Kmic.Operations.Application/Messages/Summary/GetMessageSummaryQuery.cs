using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>
/// 各模块的消息积压汇总：发件箱与收件箱的积压、最老一条的年龄、死信数
/// </summary>
/// <remarks>
/// 待终结的订阅（已经从代码里去掉、等运维确认）的积压是预期内的，带标记单独列出，
/// 界面不要把它们和普通订阅的积压混在一起告警
/// </remarks>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record GetMessageSummaryQuery : IQuery<MessageSummaryResponse>;

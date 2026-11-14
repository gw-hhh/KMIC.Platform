using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>
/// 查询各模块发件箱和收件箱的积压数量、最长等待时长及死信数
/// </summary>
/// <remarks>
/// 待终结的订阅（已经从代码里去掉、等运维确认）的积压是预期内的，带标记单独列出，
/// 界面不要把它们和普通订阅的积压混在一起告警
/// </remarks>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record GetMessageSummaryQuery : IQuery<MessageSummaryResponse>;

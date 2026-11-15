using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>
/// 查询各模块发件箱和收件箱的积压数量、最长等待时长及死信数
/// </summary>
/// <remarks>
/// 已移除声明、尚待运维确认终结的订阅，其积压会单独标记。
/// 这类积压属于预期情况，应与正常订阅分开告警。
/// </remarks>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record GetMessageSummaryQuery : IQuery<MessageSummaryResponse>;

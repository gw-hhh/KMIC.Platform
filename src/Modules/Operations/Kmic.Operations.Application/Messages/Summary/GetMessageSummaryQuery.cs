using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Summary;

/// <summary>
/// 查询各模块发件箱和收件箱的积压数量、最长等待时长及死信数
/// </summary>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record GetMessageSummaryQuery : IQuery<MessageSummaryResponse>;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Dead;

/// <summary>查询死信或已放弃消息的详情，含载荷与信封（敏感消息不返回载荷）。</summary>
/// <param name="Module">所在模块</param>
/// <param name="Box">消息箱：Outbox 或 Inbox</param>
/// <param name="Id">行 ID</param>
[RequirePermission(OperationsPermissions.MessagesView)]
[AuditedRead]
public sealed record GetDeadMessageQuery(string Module, string Box, Guid Id) : IQuery<DeadMessageDetailItem>;

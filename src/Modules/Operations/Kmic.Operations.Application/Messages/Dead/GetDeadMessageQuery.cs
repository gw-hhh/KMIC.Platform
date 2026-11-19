using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Dead;

/// <summary>
/// 查询死信或已放弃消息的详情，包含载荷与信封
/// </summary>
/// <remarks>
/// 含密消息和目录里找不到的消息不返回载荷
/// </remarks>
/// <param name="Module">所在模块</param>
/// <param name="Box">Outbox 或 Inbox</param>
/// <param name="Id">行 ID</param>
[RequirePermission(OperationsPermissions.MessagesView)]
[AuditedRead]
public sealed record GetDeadMessageQuery(string Module, string Box, Guid Id) : IQuery<DeadMessageDetailItem>;

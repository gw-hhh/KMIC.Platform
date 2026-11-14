using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Dead;

/// <summary>
/// 查询死信或已放弃消息的详情，包含载荷与信封
/// </summary>
/// <remarks>
/// 查询要求查看消息权限并记录读取审计。普通消息不得携带敏感信息，含密载荷不得通过管理接口暴露。
/// 当前存储查询直接读取载荷，尚未按含密类型过滤，含密保护仍待补齐
/// </remarks>
/// <param name="Module">所在模块</param>
/// <param name="Box">Outbox 或 Inbox</param>
/// <param name="Id">行 ID</param>
[RequirePermission(OperationsPermissions.MessagesView)]
[AuditedRead]
public sealed record GetDeadMessageQuery(string Module, string Box, Guid Id) : IQuery<DeadMessageDetailItem>;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Dead;

/// <summary>
/// 查看一条死信的全部内容，包括载荷与信封
/// </summary>
/// <remarks>
/// 载荷原样显示给有权限点的人：遮盖已经在审计采集时完成，死信载荷按设计不含敏感字段，含密消息的载荷不进库。
/// 每次查看都会留下读取审计
/// </remarks>
/// <param name="Module">所在模块</param>
/// <param name="Box">Outbox 或 Inbox</param>
/// <param name="Id">行 ID</param>
[RequirePermission(OperationsPermissions.MessagesView)]
[AuditedRead]
public sealed record GetDeadMessageQuery(string Module, string Box, Guid Id) : IQuery<DeadMessageDetailItem>;

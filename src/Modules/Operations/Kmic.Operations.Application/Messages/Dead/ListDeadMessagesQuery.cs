using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Dead;

/// <summary>
/// 查询各模块的死信，发件箱和收件箱归并后按发生时间倒序游标分页，不含载荷
/// </summary>
/// <param name="Module">模块筛选，未指定时查询全部模块</param>
/// <param name="Box">Outbox 或 Inbox</param>
/// <param name="Subscription">订阅名，指定后仅查询收件箱</param>
/// <param name="MessageType">消息名称</param>
/// <param name="Cursor">游标分页参数</param>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListDeadMessagesQuery(string? Module, string? Box, string? Subscription, string? MessageType, CursorRequest Cursor)
    : IQuery<CursorPage<DeadMessageItem>>;

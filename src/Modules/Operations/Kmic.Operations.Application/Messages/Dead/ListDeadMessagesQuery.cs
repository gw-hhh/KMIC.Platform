using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Dead;

/// <summary>
/// 查询各模块的死信
/// </summary>
/// <remarks>
/// 遍历每个模块自己的发件箱与收件箱，按发生时间倒序归并，游标分页。只列死信，不含载荷
/// </remarks>
/// <param name="Module">只看这个模块的；不给就是全部模块</param>
/// <param name="Box">Outbox 或 Inbox</param>
/// <param name="Subscription">订阅名；给了就只看收件箱</param>
/// <param name="MessageType">消息名称</param>
/// <param name="Cursor">游标分页参数</param>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListDeadMessagesQuery(string? Module, string? Box, string? Subscription, string? MessageType, CursorRequest Cursor)
    : IQuery<CursorPage<DeadMessageItem>>;

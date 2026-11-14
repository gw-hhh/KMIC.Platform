using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Blocked;

/// <summary>
/// 查询有序订阅中被死信阻塞的顺序键
/// </summary>
/// <remarks>
/// 同一顺序键中，死信会阻塞后续消息，须重试成功或经检查允许后放弃才能继续。
/// 每个模块最多返回 500 个键，按等待消息数倒序排列
/// </remarks>
/// <param name="Module">模块筛选，未指定时查询全部模块</param>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListBlockedKeysQuery(string? Module) : IQuery<IReadOnlyList<BlockedKeyItem>>;

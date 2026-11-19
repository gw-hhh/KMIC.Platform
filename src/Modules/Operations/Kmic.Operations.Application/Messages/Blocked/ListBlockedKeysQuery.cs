using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Blocked;

/// <summary>
/// 查询有序订阅中被死信阻塞的顺序键
/// </summary>
/// <param name="Module">模块筛选</param>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListBlockedKeysQuery(string? Module) : IQuery<IReadOnlyList<BlockedKeyItem>>;

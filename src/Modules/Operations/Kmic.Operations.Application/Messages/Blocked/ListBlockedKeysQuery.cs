using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Blocked;

/// <summary>
/// 被有序订阅里的死信挡住的顺序键
/// </summary>
/// <remarks>
/// 有序订阅里，同一个键下排在死信后面的消息都在等它：处理掉那条死信（重试，或确认不需要而放弃）才能放开。
/// 每个模块最多列 500 个键，等得最多的排前面
/// </remarks>
/// <param name="Module">只看这个模块的；不给就是全部模块</param>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListBlockedKeysQuery(string? Module) : IQuery<IReadOnlyList<BlockedKeyItem>>;

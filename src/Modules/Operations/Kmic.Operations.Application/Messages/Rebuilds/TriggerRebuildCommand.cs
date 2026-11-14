using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 手动触发订阅读模型重建
/// </summary>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record TriggerRebuildCommand(string Subscription) : ICommand;

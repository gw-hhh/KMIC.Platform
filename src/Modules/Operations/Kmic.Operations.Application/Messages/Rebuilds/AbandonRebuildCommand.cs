using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 放弃等待确认的读模型重建
/// </summary>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record AbandonRebuildCommand(string Subscription) : ICommand;

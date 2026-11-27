using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>手动触发订阅读模型重建。</summary>
/// <param name="Subscription">订阅名</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
[ExemptFromMessagingSuspension]
public sealed record TriggerRebuildCommand(string Subscription) : ICommand;

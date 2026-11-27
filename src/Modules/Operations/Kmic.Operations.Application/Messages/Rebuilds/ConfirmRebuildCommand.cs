using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>确认读模型重建收尾。</summary>
/// <param name="Subscription">订阅名</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
[ExemptFromMessagingSuspension]
public sealed record ConfirmRebuildCommand(string Subscription) : ICommand;

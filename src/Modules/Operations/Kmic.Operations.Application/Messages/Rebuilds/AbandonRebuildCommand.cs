using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 放弃暂停的一轮读模型重建，此后不会自动重建
/// </summary>
/// <remarks>
/// 放弃是运维明确的决定，同时清除订阅的“需要初始化”标记，之后每次启动都不会再自动排队；要重建由运维手动触发
/// </remarks>
/// <param name="Subscription">订阅名</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record AbandonRebuildCommand(string Subscription) : ICommand;

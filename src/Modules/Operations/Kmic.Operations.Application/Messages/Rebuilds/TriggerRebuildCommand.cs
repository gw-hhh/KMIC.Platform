using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 给订阅手动排一轮读模型重建
/// </summary>
/// <remarks>
/// 订阅要声明了重建方式。已经有排队或进行中、暂停等确认的一轮时拒绝：先处理它
/// </remarks>
/// <param name="Subscription">订阅名</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record TriggerRebuildCommand(string Subscription) : ICommand;

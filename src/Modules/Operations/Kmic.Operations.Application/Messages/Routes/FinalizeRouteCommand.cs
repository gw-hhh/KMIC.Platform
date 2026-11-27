using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>终结消息订阅路由（须在维护窗口执行）。</summary>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
/// <param name="Reason">原因</param>
/// <param name="AbandonUnfinished">是否放弃未完成的消息</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
[ExemptFromMessagingSuspension]
public sealed record FinalizeRouteCommand(string MessageType, string Subscription, string Reason, bool AbandonUnfinished) : ICommand<FinalizeRouteResponse>;

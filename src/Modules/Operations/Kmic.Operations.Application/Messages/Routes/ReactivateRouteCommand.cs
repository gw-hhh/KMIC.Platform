using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 重新激活已终结的订阅
/// </summary>
/// <remarks>
/// 终结期间的消息不会自动补投，要核对缺口后按来源序号补投；要先重新激活，再启动声明该订阅的构建
/// </remarks>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record ReactivateRouteCommand(string MessageType, string Subscription) : ICommand;

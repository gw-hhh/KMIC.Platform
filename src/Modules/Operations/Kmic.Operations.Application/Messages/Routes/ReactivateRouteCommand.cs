using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 重新激活一个已终结的订阅：恢复成生效，从现在起重新投递
/// </summary>
/// <remarks>
/// 终结期间漏掉的消息不会自动补回，要核对缺口区间，再用补投按序号区间补。
/// 之后声明这个订阅的构建才能启动
/// </remarks>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record ReactivateRouteCommand(string MessageType, string Subscription) : ICommand;

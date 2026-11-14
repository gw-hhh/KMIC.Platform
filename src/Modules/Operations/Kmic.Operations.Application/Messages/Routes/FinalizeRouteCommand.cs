using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 终结一个待终结的订阅：之后不再向它投递
/// </summary>
/// <remarks>
/// 终结意味着跨过回滚边界：旧构建不能再自动恢复这个订阅，所以要在受控的维护窗口里做。
/// 路由要先是待终结（代码里声明了退役）、没有运行中的实例还在声明它、没有消息正在处理。
/// 还有没处理完的消息时，要选择把它们放弃，原因连同原来的错误一起记下；不选择就拒绝终结
/// </remarks>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
/// <param name="Reason">原因，必填</param>
/// <param name="AbandonUnfinished">有没处理完的消息时，是否把它们放弃</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record FinalizeRouteCommand(string MessageType, string Subscription, string Reason, bool AbandonUnfinished) : ICommand<FinalizeRouteResponse>;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 按来源序号区间补投消息
/// </summary>
/// <remarks>
/// 只补投来源发件箱里已投递的消息，已接收的不重复写入；已终结的订阅要先重新激活；区间两端都包含
/// </remarks>
/// <param name="SourceModule">发布消息的模块</param>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
/// <param name="FromSeq">起始序号，含</param>
/// <param name="ToSeq">结束序号，含</param>
[RequirePermission(OperationsPermissions.MessagesManage)]
[Audited]
[Idempotent]
public sealed record RedeliverCommand(string SourceModule, string MessageType, string Subscription, long FromSeq, long ToSeq) : ICommand<RedeliverResponse>;

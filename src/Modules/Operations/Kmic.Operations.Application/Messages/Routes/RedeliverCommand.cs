using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 补投：把来源模块发件箱里已经投递过的、序号落在区间里的消息，再投递给指定订阅
/// </summary>
/// <remarks>
/// 订阅收到过的消息不会重复写入。一次最多补投 5000 条，超过要缩小序号区间。
/// 订阅必须是生效或待终结的，已终结的要先重新激活。序号是来源模块发件箱里的序号
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

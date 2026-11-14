namespace Kmic.Operations.Presentation.Messages;

/// <summary>
/// 补投的请求
/// </summary>
/// <param name="SourceModule">发布消息的模块</param>
/// <param name="MessageType">消息名称</param>
/// <param name="Subscription">订阅名</param>
/// <param name="FromSeq">起始序号，含，是来源模块发件箱里的序号</param>
/// <param name="ToSeq">结束序号，含</param>
public sealed record RedeliverRequest(string SourceModule, string MessageType, string Subscription, long FromSeq, long ToSeq);

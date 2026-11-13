using System.Text.Json;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages;

/// <summary>
/// 一条死信的全部内容：概要加载荷与信封
/// </summary>
/// <param name="Message">概要</param>
/// <param name="State">状态：死信，或者已经被放弃</param>
/// <param name="Payload">载荷。按设计不含敏感字段，含密消息（验证码之类）的载荷不进这里；每次查看都会留下读取审计</param>
/// <param name="Headers">信封里的其余字段</param>
public sealed record DeadMessageDetailItem(DeadMessageItem Message, MessageState State, JsonElement Payload, JsonElement Headers);

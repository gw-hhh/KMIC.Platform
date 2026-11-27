using System.Text.Json;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages;

/// <summary>死信或已放弃消息详情（含概要、载荷与信封）。</summary>
/// <param name="Message">概要</param>
/// <param name="State">状态</param>
/// <param name="Payload">消息载荷，敏感消息时为空</param>
/// <param name="Headers">信封其余字段</param>
public sealed record DeadMessageDetailItem(DeadMessageItem Message, MessageState State, JsonElement? Payload, JsonElement Headers);

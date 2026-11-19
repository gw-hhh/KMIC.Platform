using System.Text.Json;
using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages;

/// <summary>
/// 死信或已放弃消息的详情，包含概要、载荷和信封
/// </summary>
/// <param name="Message">概要</param>
/// <param name="State">状态</param>
/// <param name="Payload">消息载荷，为空</param>
/// <param name="Headers">信封里的其余字段</param>
public sealed record DeadMessageDetailItem(DeadMessageItem Message, MessageState State, JsonElement? Payload, JsonElement Headers);

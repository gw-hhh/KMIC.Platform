using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages;

/// <summary>
/// 要重试或放弃的一条消息
/// </summary>
/// <param name="Module">所在模块</param>
/// <param name="Box">在发件箱还是收件箱</param>
/// <param name="Id">行 ID</param>
public sealed record MessageRef(string Module, MessageBoxKind Box, Guid Id);

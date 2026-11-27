using Kmic.Operations.Domain.Messages;

namespace Kmic.Operations.Application.Messages;

/// <summary>待重试或放弃的消息标识。</summary>
/// <param name="Module">所在模块</param>
/// <param name="Box">所属消息箱（发件箱或收件箱）</param>
/// <param name="Id">行 ID</param>
public sealed record MessageRef(string Module, MessageBoxKind Box, Guid Id);

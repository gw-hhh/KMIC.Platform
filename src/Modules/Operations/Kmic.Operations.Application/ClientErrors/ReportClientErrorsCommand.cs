using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>
/// 批量上报前端错误
/// </summary>
/// <remarks>
/// 必须修改密码的账号和只读模拟会话也可上报。超限返回 429，前端应丢弃该批次，不重试
/// </remarks>
/// <param name="Errors">一批错误，条数上限见配置</param>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[AllowWhenImpersonationReadOnly]
[NotAudited("上报内容已记入错误日志")]
[NotIdempotent("仅写入错误日志，无需缓存上报响应")]
public sealed record ReportClientErrorsCommand(IReadOnlyList<ClientErrorReport> Errors) : ICommand;

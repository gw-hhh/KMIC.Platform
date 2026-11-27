using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>批量上报前端错误（超限时返回 429，前端不应重试）。</summary>
/// <param name="Errors">错误列表</param>
[AllowAuthenticated]
[AllowWhenPasswordChangeRequired]
[AllowWhenImpersonationReadOnly]
[NotAudited("上报内容已记入错误日志")]
[NotIdempotent("仅写入错误日志，无需缓存上报响应")]
public sealed record ReportClientErrorsCommand(IReadOnlyList<ClientErrorReport> Errors) : ICommand;

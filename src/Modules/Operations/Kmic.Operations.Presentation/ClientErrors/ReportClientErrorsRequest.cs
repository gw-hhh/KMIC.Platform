using Kmic.Operations.Application.ClientErrors;

namespace Kmic.Operations.Presentation.ClientErrors;

/// <summary>
/// 前端批量上报错误的请求
/// </summary>
/// <param name="Errors">一批错误；一次最多 20 条，更多的拆成几次。字段白名单，未知字段整个请求被拒绝</param>
public sealed record ReportClientErrorsRequest(IReadOnlyList<ClientErrorReport> Errors);

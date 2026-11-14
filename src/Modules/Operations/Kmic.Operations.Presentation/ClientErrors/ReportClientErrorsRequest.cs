using Kmic.Operations.Application.ClientErrors;

namespace Kmic.Operations.Presentation.ClientErrors;

/// <summary>
/// 前端批量上报错误的请求
/// </summary>
/// <param name="Errors">本批错误，默认每次最多 20 条，实际以配置为准；超出时须分批上报。仅接受已声明字段，未知字段会导致整次请求被拒绝</param>
public sealed record ReportClientErrorsRequest(IReadOnlyList<ClientErrorReport> Errors);

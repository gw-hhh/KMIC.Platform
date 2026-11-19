using Kmic.Operations.Application.ClientErrors;

namespace Kmic.Operations.Presentation.ClientErrors;

/// <summary>
/// 前端批量上报错误的请求
/// </summary>
/// <param name="Errors">本批错误</param>
public sealed record ReportClientErrorsRequest(IReadOnlyList<ClientErrorReport> Errors);

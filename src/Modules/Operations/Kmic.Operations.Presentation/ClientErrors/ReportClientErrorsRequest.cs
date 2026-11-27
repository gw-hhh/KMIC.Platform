using Kmic.Operations.Application.ClientErrors;

namespace Kmic.Operations.Presentation.ClientErrors;

/// <param name="Errors">错误列表</param>
public sealed record ReportClientErrorsRequest(IReadOnlyList<ClientErrorReport> Errors);

using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Domain.Errors;

/// <summary>
/// Operations 模块的错误目录
/// </summary>
/// <remarks>
/// 错误码写成“Operations.错误名”
/// </remarks>
public static class OperationsErrors
{
    /// <summary>
    /// 审计记录不存在
    /// </summary>
    public static readonly Error AuditNotFound = Error.NotFound("Operations.AuditNotFound", "审计记录不存在");
}

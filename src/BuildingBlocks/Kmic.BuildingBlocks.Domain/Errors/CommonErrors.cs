using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Domain.Errors;

/// <summary>
/// 框架自身的错误目录，错误码前缀为 Common
/// </summary>
/// <remarks>
/// 应用层与基础设施的通用错误在实现对应能力时加到这里（05-02 第 8 节）
/// </remarks>
public static class CommonErrors
{
    /// <summary>
    /// 操作所依据的规则已变化，需要重新确认；为将来的版本化规则集预留
    /// </summary>
    public static readonly Error RulesChanged =
        Error.Conflict("Common.RulesChanged", "相关规则已更新，请刷新后重新确认");
}

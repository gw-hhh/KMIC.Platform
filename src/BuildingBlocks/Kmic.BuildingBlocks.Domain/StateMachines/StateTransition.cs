namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// 状态流转记录
/// </summary>
/// <param name="StateName">状态属性名，如 Status</param>
/// <param name="FromCode">原状态编码，创建时为空</param>
/// <param name="ToCode">新状态编码</param>
/// <param name="ActionCode">动作编码；创建时为 StateTransition.CreateActionCode</param>
/// <param name="Reason">原因</param>
/// <param name="Revision">业务版本号</param>
public sealed record StateTransition(
    string StateName,
    string? FromCode,
    string ToCode,
    string ActionCode,
    string? Reason,
    int? Revision)
{
    /// <summary>
    /// 创建时的动作编码
    /// </summary>
    public const string CreateActionCode = "Create";
}

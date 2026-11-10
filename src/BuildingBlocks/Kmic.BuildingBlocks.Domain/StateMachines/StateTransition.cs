namespace Kmic.BuildingBlocks.Domain.StateMachines;

/// <summary>
/// 一次状态流转的记录；操作人、时间、追踪 ID 由保存时的拦截器补上后写入业务历史
/// </summary>
/// <param name="StateName">被状态机管理的属性名，如 Status；一个聚合有多个状态属性时用于区分</param>
/// <param name="FromCode">原状态编码，创建时为空</param>
/// <param name="ToCode">新状态编码</param>
/// <param name="ActionCode">动作编码；创建时为 StateTransition.CreateActionCode</param>
/// <param name="Reason">用户填写的原因，可为空</param>
/// <param name="Revision">聚合实现 IHasRevision 时的业务版本，否则为空</param>
public sealed record StateTransition(
    string StateName,
    string? FromCode,
    string ToCode,
    string ActionCode,
    string? Reason,
    int? Revision)
{
    /// <summary>
    /// 状态机设置初始状态时记录的动作编码
    /// </summary>
    public const string CreateActionCode = "Create";
}

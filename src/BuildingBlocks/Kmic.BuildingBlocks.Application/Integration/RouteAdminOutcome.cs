namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 路由运维操作的结果
/// </summary>
public enum RouteAdminOutcome
{
    /// <summary>
    /// 做完了
    /// </summary>
    Done,

    /// <summary>
    /// 没有这条路由
    /// </summary>
    NotFound,

    /// <summary>
    /// 路由当前的状态不能做这个操作：终结要待终结的，重新激活要已终结的
    /// </summary>
    WrongStatus,

    /// <summary>
    /// 还有运行中的实例在声明这个订阅
    /// </summary>
    StillDeclared,

    /// <summary>
    /// 收件箱里有正在处理的消息，占用还没到期
    /// </summary>
    InFlight,

    /// <summary>
    /// 还有没处理完的消息，而没有选择放弃
    /// </summary>
    HasUnfinished,

    /// <summary>
    /// 来源模块没有注册
    /// </summary>
    UnknownSourceModule,

    /// <summary>
    /// 区间里的消息太多，一次补不完
    /// </summary>
    TooManyRows,
}

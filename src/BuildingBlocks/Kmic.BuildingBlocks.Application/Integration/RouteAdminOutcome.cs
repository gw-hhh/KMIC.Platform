namespace Kmic.BuildingBlocks.Application.Integration;

public enum RouteAdminOutcome
{
    Done,

    NotFound,

    /// <summary>路由当前状态不允许该操作。</summary>
    WrongStatus,

    /// <summary>仍有运行中的实例正在声明该订阅。</summary>
    StillDeclared,

    /// <summary>收件箱中仍有处理中的消息且占用尚未到期。</summary>
    InFlight,

    /// <summary>仍存在未处理完成的消息，且未选择强制放弃。</summary>
    HasUnfinished,

    /// <summary>来源模块未注册。</summary>
    UnknownSourceModule,

    /// <summary>指定区间内消息数量超出单次补投上限。</summary>
    TooManyRows,
}

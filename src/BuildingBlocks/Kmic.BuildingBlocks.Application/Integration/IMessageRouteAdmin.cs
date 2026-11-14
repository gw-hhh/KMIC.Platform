using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 订阅路由的运维操作：终结与重新激活
/// </summary>
/// <remarks>
/// 给 Operations 的管理命令用。路由表属于 Operations，订阅方的收件箱属于订阅方模块，终结要同时改两边，
/// 所以由框架在同一个事务里完成，和投递程序改状态一样是基础设施的写入，不在命令的事务里
/// </remarks>
public interface IMessageRouteAdmin
{
    /// <summary>
    /// 终结一个待终结的订阅：之后不再向它投递，剩下的未完成消息按选择放弃
    /// </summary>
    /// <remarks>
    /// 先排除不能终结的情况：路由不是待终结；还有运行中的实例在声明它；收件箱里有正在处理的消息（占用还没到期）；
    /// 有没处理完的消息而没有选择放弃。都没有才改状态
    /// </remarks>
    /// <param name="messageType">消息名称</param>
    /// <param name="subscription">订阅名</param>
    /// <param name="by">终结的人</param>
    /// <param name="reason">原因，放弃剩余消息时连同原来的错误一起记下</param>
    /// <param name="abandonUnfinished">有没处理完的消息时，是否把它们放弃</param>
    /// <param name="instanceAliveWithin">多久之内有心跳的实例算还在运行</param>
    /// <param name="ct">取消令牌</param>
    Task<RouteFinalizeResult> FinalizeAsync(
        string messageType, string subscription, UserId by, string reason, bool abandonUnfinished, TimeSpan instanceAliveWithin, CancellationToken ct);

    /// <summary>
    /// 重新激活一个已终结的订阅：恢复成生效，从现在起重新投递
    /// </summary>
    /// <remarks>
    /// 终结期间漏掉的消息不会自动补回，要用补投按序号区间补
    /// </remarks>
    /// <param name="messageType">消息名称</param>
    /// <param name="subscription">订阅名</param>
    /// <param name="ct">取消令牌</param>
    Task<RouteAdminOutcome> ReactivateAsync(string messageType, string subscription, CancellationToken ct);

    /// <summary>
    /// 补投：把来源模块发件箱里已经投递过的、序号落在区间里的消息，再写进指定订阅的收件箱
    /// </summary>
    /// <remarks>
    /// 和正常投递一样先锁住来源行再写入，所以来源行被清理之后不会再有迟到的补投；订阅已经处理过的消息由收件箱的去重凭证挡住，
    /// 不会重复执行。订阅必须是生效或待终结的，已终结的要先重新激活。一次最多 <see cref="MaxRedeliverRows" /> 条，超过要缩小区间
    /// </remarks>
    /// <param name="sourceModule">发布消息的模块，序号是它发件箱里的序号</param>
    /// <param name="messageType">消息名称</param>
    /// <param name="subscription">订阅名</param>
    /// <param name="fromSeq">起始序号，含</param>
    /// <param name="toSeq">结束序号，含</param>
    /// <param name="ct">取消令牌</param>
    Task<RedeliverResult> RedeliverAsync(string sourceModule, string messageType, string subscription, long fromSeq, long toSeq, CancellationToken ct);

    /// <summary>
    /// 一次补投最多多少条
    /// </summary>
    const int MaxRedeliverRows = 5000;
}

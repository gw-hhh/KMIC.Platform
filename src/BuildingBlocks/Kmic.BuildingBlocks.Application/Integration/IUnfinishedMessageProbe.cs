namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 探查还有哪些对象在未完成的消息里被提到
/// </summary>
/// <remarks>
/// 给“删除某个对象之前，确认没有指向它的消息还在路上”用，如 Files 清理文件前探查未完成的关联请求。
/// 只能在命令里用，查询走这个命令的事务连接，读到的是主实例上已经提交的最新数据，不经任何缓存。
/// 按消息前进的方向先查各模块的发件箱（待投递、投递中、死信），再查各模块的收件箱（待处理、处理中、死信）：
/// 消息只会沿“发件箱 → 收件箱 → 已处理”单向前进，这样后一次查询的快照不早于前一次，不会从两次查询之间漏过
/// </remarks>
public interface IUnfinishedMessageProbe
{
    /// <summary>
    /// 找出被某类未完成的消息提到的对象
    /// </summary>
    /// <param name="messageType">消息类型名，如 Files.AttachFiles</param>
    /// <param name="subjectIds">要探查的对象 ID；消息要用 [MessageSubjects] 声明了主体才查得到</param>
    /// <param name="ct">取消令牌</param>
    /// <returns>有未完成消息提到的那些 ID</returns>
    Task<IReadOnlySet<Guid>> FindSubjectsAsync(string messageType, IReadOnlyCollection<Guid> subjectIds, CancellationToken ct);
}

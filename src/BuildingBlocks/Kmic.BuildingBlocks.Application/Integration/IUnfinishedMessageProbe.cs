namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 探查哪些对象还在未完成的消息里
/// </summary>
/// <remarks>
/// 只能在命令里用
/// </remarks>
public interface IUnfinishedMessageProbe
{
    /// <summary>
    /// 找出被某类未完成的消息提到的对象
    /// </summary>
    /// <returns>有未完成消息提到的那些 ID</returns>
    Task<IReadOnlySet<Guid>> FindSubjectsAsync(string messageType, IReadOnlyCollection<Guid> subjectIds, CancellationToken ct);
}

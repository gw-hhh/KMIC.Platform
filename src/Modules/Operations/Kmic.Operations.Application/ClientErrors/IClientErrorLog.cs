using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>
/// 前端错误的日志出口：日志分类 Kmic.ClientErrors，Warning 级结构化日志
/// </summary>
public interface IClientErrorLog
{
    /// <summary>
    /// 写一条前端上报的错误
    /// </summary>
    /// <param name="user">上报的用户，日志里只记 ID</param>
    /// <param name="report">错误</param>
    void Write(UserId user, ClientErrorReport report);
}

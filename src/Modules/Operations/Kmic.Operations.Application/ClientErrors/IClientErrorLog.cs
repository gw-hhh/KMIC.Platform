using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>
/// 将前端错误写入 Kmic.ClientErrors 分类的 Warning 级结构化日志
/// </summary>
public interface IClientErrorLog
{
    /// <summary>
    /// 写一条前端上报的错误
    /// </summary>
    void Write(UserId user, ClientErrorReport report);
}

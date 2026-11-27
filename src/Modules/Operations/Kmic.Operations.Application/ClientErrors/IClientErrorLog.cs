using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.Operations.Application.ClientErrors;

/// <summary>将前端错误写入 Kmic.ClientErrors 分类的 Warning 级结构化日志。</summary>
public interface IClientErrorLog
{
    void Write(UserId user, ClientErrorReport report);
}

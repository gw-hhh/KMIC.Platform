using Kmic.BuildingBlocks.Application.Access;

namespace Kmic.BuildingBlocks.Application.Execution;

public interface IExecutionContext
{
    CallOrigin Origin { get; }

    ICurrentUser User { get; }

    /// <summary>访问上下文；系统身份或匿名请求时为 null。</summary>
    IAccessContext? Access { get; }

    bool InCommand { get; }
}

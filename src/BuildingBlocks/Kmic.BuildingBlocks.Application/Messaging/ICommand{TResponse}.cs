using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 有返回值的命令
/// </summary>
/// <remarks>
/// 只返回 ID、版本号、状态这类最少的信息，业务内容通过查询获取
/// </remarks>
public interface ICommand<TResponse> : Mediator.ICommand<Result<TResponse>>;

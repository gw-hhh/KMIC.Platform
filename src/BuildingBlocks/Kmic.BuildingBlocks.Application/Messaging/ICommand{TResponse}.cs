using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>有返回值的命令；仅返回 ID、版本号、状态等最小必要信息。</summary>
public interface ICommand<TResponse> : Mediator.ICommand<Result<TResponse>>;

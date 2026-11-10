using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 修改数据并返回收据的命令；收据只含标识、版本、状态编码等最小信息，业务内容经查询获取
/// </summary>
public interface ICommand<TResponse> : Mediator.ICommand<Result<TResponse>>;

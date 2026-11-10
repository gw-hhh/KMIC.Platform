using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Messaging;

/// <summary>
/// 修改数据、不返回值的命令；经过幂等与事务行为
/// </summary>
public interface ICommand : Mediator.ICommand<Result>;

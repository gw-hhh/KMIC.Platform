using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Instances;

/// <summary>
/// 查询消息处理实例、订阅及心跳状态
/// </summary>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListInstancesQuery : IQuery<IReadOnlyList<InstanceItem>>;

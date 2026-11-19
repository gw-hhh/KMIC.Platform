using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Instances;

/// <summary>
/// 查询消息处理实例、订阅及心跳状态
/// </summary>
/// <remarks>
/// 只用于诊断：判断哪些构建还在声明某个订阅、能不能终结它；最近有心跳的排前面
/// </remarks>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListInstancesQuery : IQuery<IReadOnlyList<InstanceItem>>;

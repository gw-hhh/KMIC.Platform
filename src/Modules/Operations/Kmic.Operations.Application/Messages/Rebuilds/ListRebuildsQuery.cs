using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 查询读模型重建的各轮：每个订阅最近几轮，新的在前
/// </summary>
/// <remarks>
/// 暂停等确认的一轮要运维核实后确认收尾或放弃
/// </remarks>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListRebuildsQuery : IQuery<IReadOnlyList<RebuildItem>>;

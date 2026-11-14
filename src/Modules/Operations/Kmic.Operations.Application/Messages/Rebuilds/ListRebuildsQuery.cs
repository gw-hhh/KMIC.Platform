using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Rebuilds;

/// <summary>
/// 查询最近的读模型重建记录
/// </summary>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListRebuildsQuery : IQuery<IReadOnlyList<RebuildItem>>;

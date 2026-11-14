using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 查询消息订阅路由
/// </summary>
/// <remarks>
/// 不分页，按模块、订阅名和消息类型排序
/// </remarks>
/// <param name="Status">状态编码，可以给多个；不给就是全部</param>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListRoutesQuery(IReadOnlyList<string>? Status) : IQuery<IReadOnlyList<RouteItem>>;

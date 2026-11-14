using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>
/// 查询订阅路由：哪种消息投递给哪个模块的哪个订阅，状态，以及还有哪些构建在声明它
/// </summary>
/// <remarks>
/// 路由数量不大，一次全部返回，按模块、订阅排序
/// </remarks>
/// <param name="Status">状态编码，可以给多个；不给就是全部</param>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListRoutesQuery(IReadOnlyList<string>? Status) : IQuery<IReadOnlyList<RouteItem>>;

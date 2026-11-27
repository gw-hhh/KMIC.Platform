using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.Operations.Application.Messages.Routes;

/// <summary>查询全部消息订阅路由（按模块、订阅名与消息类型排序）。</summary>
/// <param name="Status">状态编码</param>
[RequirePermission(OperationsPermissions.MessagesView)]
public sealed record ListRoutesQuery(IReadOnlyList<string>? Status) : IQuery<IReadOnlyList<RouteItem>>;

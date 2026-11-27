using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Instances;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class ListInstancesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/messages/instances", HandleAsync)
            .Sends<ListInstancesQuery>()
            .WithSummary("查询消息处理实例")
            .WithDescription("按最近心跳倒序返回实例构建号、角色、心跳与订阅（含过期记录，用于核对订阅声明）");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListInstancesQuery(), ct)).ToOk();
}

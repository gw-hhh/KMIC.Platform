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
            .WithDescription("列出实例的构建号、角色、心跳和订阅，按最近心跳倒序。包含心跳已过期的记录，供诊断订阅声明情况。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListInstancesQuery(), ct)).ToOk();
}

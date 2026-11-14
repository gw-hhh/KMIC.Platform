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
            .WithSummary("查询运行中的实例")
            .WithDescription("每个实例的构建号、角色、最近一次心跳和它声明的订阅，最近有心跳的排前面。只用于诊断：判断哪些构建还在声明某个订阅。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListInstancesQuery(), ct)).ToOk();
}

using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Routes;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class ListRoutesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/messages/routes", HandleAsync)
            .Sends<ListRoutesQuery>()
            .WithSummary("查询订阅路由")
            .WithDescription("返回消息类型、订阅方模块、订阅名及路由状态，并列出心跳未过期且声明该订阅的构建，供终结前核对。");

    private static async Task<IResult> HandleAsync([Description("状态编码，多值须重复参数名；未指定时查询全部状态")] string[]? status, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListRoutesQuery(status), ct)).ToOk();
}

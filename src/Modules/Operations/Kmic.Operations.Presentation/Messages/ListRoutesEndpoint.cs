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
            .WithDescription("哪种消息投递给哪个模块的哪个订阅，状态（生效、待终结、已终结），以及现在还在运行的实例里哪些构建声明了它。终结一个订阅之前先看这里。");

    private static async Task<IResult> HandleAsync([Description("状态编码，重复参数名可以给多个，如 status=Retiring；不给就是全部")] string[]? status, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListRoutesQuery(status), ct)).ToOk();
}

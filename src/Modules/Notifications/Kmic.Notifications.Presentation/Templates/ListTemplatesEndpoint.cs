using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Templates.List;

namespace Kmic.Notifications.Presentation.Templates;

internal sealed class ListTemplatesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/templates", HandleAsync)
            .Sends<ListTemplatesQuery>()
            .WithSummary("查询全部通知模板")
            .WithDescription("每个模板的渠道、状态、各渠道生效的版本号和最近一次发布的时间。没有发布过版本的渠道用代码里的默认内容。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListTemplatesQuery(), ct)).ToOk();
}

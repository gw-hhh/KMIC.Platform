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
            .WithDescription("返回模板渠道、状态、各渠道选用的发布版本及最近发布时间。活动模板未选用发布版本的渠道使用代码默认内容。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListTemplatesQuery(), ct)).ToOk();
}

using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Templates.Get;

namespace Kmic.Notifications.Presentation.Templates;

internal sealed class GetTemplateEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/templates/{key}", HandleAsync)
            .Sends<GetTemplateQuery>()
            .WithSummary("查询通知模板详情")
            .WithDescription("返回渠道内容、代码默认内容、变量、示例数据与已发布版本（已移除声明的模板不含默认值与变量）；发布或恢复时须回传 version");

    private static async Task<IResult> HandleAsync([Description("模板键，如 Sample.SpecimenSubmitted")] string key, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetTemplateQuery(key), ct)).ToOk();
}

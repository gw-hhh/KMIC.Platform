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
            .WithSummary("查询一个通知模板")
            .WithDescription("各渠道当前的内容和代码里的默认内容、模板里能用的变量、示例数据、发布过的版本。响应里的 version 在发布和恢复时作为期望版本传回。");

    private static async Task<IResult> HandleAsync([Description("模板键，如 Sample.SpecimenSubmitted")] string key, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetTemplateQuery(key), ct)).ToOk();
}

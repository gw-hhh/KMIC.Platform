using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Templates.Preview;

namespace Kmic.Notifications.Presentation.Templates;

internal sealed class PreviewTemplateEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/templates/{key}/preview", HandleAsync)
            .Sends<PreviewTemplateQuery>()
            .WithSummary("预览模板内容的渲染结果")
            .WithDescription("仅渲染预览不保存；渲染失败仍返回 200（原因见 error 字段），模板不存在或入参校验失败返回对应错误");

    private static async Task<IResult> HandleAsync([Description("模板键")] string key, PreviewTemplateRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PreviewTemplateQuery(key, request.Channel, request.Subject, request.Body, request.Model), ct)).ToOk();
}

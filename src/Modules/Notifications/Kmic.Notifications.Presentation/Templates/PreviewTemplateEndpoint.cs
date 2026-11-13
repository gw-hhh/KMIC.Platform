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
            .WithDescription("不保存。渲染的限时和输出上限与正式发送一样。模板有误时仍返回 200，原因放在 error 里，方便边写边看。");

    private static async Task<IResult> HandleAsync([Description("模板键")] string key, PreviewTemplateRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PreviewTemplateQuery(key, request.Channel, request.Subject, request.Body, request.Model), ct)).ToOk();
}

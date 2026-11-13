using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Templates.Publish;

namespace Kmic.Notifications.Presentation.Templates;

internal sealed class PublishTemplateEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/templates/{key}/publish", HandleAsync)
            .Sends<PublishTemplateCommand>()
            .WithSummary("给模板的一个渠道发布新版本")
            .WithDescription("发布后立即生效。发布前用示例数据渲染一遍，语法错误、超时、输出过大返回 422。已退役的模板不能发布。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("模板键")] string key, PublishTemplateRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PublishTemplateCommand(key, request.Channel, request.Subject, request.Body, request.Remark, request.ExpectedVersion), ct)).ToOk();
}

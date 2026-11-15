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
            .WithSummary("发布指定渠道的模板新版本")
            .WithDescription("发布后，后续渲染使用新版本，已生成的投递内容不变。发布前用示例数据严格渲染，语法错误、遇到未定义变量或超限时返回 422；未执行的条件分支不在校验范围。已退役的模板不能发布。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("模板键")] string key, PublishTemplateRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PublishTemplateCommand(key, request.Channel, request.Subject, request.Body, request.Remark, request.ExpectedVersion), ct)).ToOk();
}

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
            .WithDescription("发布前按示例数据严格校验（语法错、未定义变量或超限返回 422，未执行分支除外），仅影响新投递；已废弃模板不可发布");

    private static async Task<IResult> HandleAsync([Description("模板键")] string key, PublishTemplateRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PublishTemplateCommand(key, request.Channel, request.Subject, request.Body, request.Remark, request.ExpectedVersion), ct)).ToOk();
}

using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Templates.Revert;

namespace Kmic.Notifications.Presentation.Templates;

internal sealed class RevertTemplateEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/templates/{key}/revert", HandleAsync)
            .Sends<RevertTemplateCommand>()
            .WithSummary("恢复指定渠道的模板默认内容")
            .WithDescription("恢复指定渠道的代码默认内容，保留已发布的历史版本。");

    private static async Task<IResult> HandleAsync([Description("模板键")] string key, RevertTemplateRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RevertTemplateCommand(key, request.Channel, request.ExpectedVersion), ct)).ToOk();
}

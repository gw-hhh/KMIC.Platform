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
            .WithSummary("恢复模板某个渠道的默认内容")
            .WithDescription("这个渠道改回用代码里的内容，发布过的历史版本保留。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("模板键")] string key, RevertTemplateRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RevertTemplateCommand(key, request.Channel, request.ExpectedVersion), ct)).ToOk();
}

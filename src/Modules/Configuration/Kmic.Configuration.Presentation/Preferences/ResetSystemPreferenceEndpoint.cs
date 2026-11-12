using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Preferences.Layers;

namespace Kmic.Configuration.Presentation.Preferences;

internal sealed class ResetSystemPreferenceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/preferences/{key}/layers/system/reset", HandleAsync)
            .Sends<ResetPreferenceLayerCommand>()
            .WithSummary("删除偏好的系统层")
            .WithDescription("只删系统层，代码里的默认值重新对没有被覆盖的人生效。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, ResetPreferenceLayerRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetPreferenceLayerCommand(key, null, request.ExpectedVersion), ct)).ToOk();
}

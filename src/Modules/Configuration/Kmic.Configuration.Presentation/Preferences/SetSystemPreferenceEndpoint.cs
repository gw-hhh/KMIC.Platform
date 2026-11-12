using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Preferences.Layers;

namespace Kmic.Configuration.Presentation.Preferences;

internal sealed class SetSystemPreferenceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/preferences/{key}/layers/system", HandleAsync)
            .Sends<SetPreferenceLayerCommand>()
            .WithSummary("设置偏好的系统层")
            .WithDescription("对所有人生效，没有被角色层或本人覆盖的人才会看到。声明里不允许系统层的偏好会被拒绝。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, SetPreferenceLayerRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetPreferenceLayerCommand(key, null, request.Value, request.ExpectedVersion), ct)).ToOk();
}

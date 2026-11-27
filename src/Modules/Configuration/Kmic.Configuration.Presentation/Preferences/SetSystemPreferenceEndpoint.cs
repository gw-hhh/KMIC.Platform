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
            .WithSummary("设置系统级偏好")
            .WithDescription("偏好声明须允许系统层设置；有效的个人或角色设置仍优先");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, SetPreferenceLayerRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetPreferenceLayerCommand(key, null, request.Value, request.ExpectedVersion), ct)).ToOk();
}

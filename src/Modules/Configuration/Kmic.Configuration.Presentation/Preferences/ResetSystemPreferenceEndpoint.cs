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
            .WithSummary("移除系统级偏好设置")
            .WithDescription("移除系统层偏好设置；无个人或角色层有效值时回退至代码默认值");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, ResetPreferenceLayerRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetPreferenceLayerCommand(key, null, request.ExpectedVersion), ct)).ToOk();
}

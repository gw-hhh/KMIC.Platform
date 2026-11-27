using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Preferences.Layers;

namespace Kmic.Configuration.Presentation.Preferences;

internal sealed class SetRolePreferenceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/preferences/{key}/layers/roles/{roleId}", HandleAsync)
            .Sends<SetPreferenceLayerCommand>()
            .WithSummary("设置角色级偏好")
            .WithDescription("角色须存在且偏好声明允许角色层设置；有效的个人设置仍优先");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, [Description("角色 ID")] RoleId roleId, SetPreferenceLayerRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetPreferenceLayerCommand(key, roleId, request.Value, request.ExpectedVersion), ct)).ToOk();
}

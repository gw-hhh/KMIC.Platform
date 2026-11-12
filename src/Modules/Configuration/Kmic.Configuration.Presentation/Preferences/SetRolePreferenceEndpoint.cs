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
            .WithSummary("设置偏好在一个角色上的值")
            .WithDescription("这个角色下没有自己设置过的人会看到它。角色必须存在。声明里不允许角色层的偏好会被拒绝。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, [Description("角色 ID")] RoleId roleId, SetPreferenceLayerRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetPreferenceLayerCommand(key, roleId, request.Value, request.ExpectedVersion), ct)).ToOk();
}

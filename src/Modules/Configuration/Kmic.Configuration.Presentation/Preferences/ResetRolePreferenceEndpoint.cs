using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Preferences.Layers;

namespace Kmic.Configuration.Presentation.Preferences;

internal sealed class ResetRolePreferenceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/preferences/{key}/layers/roles/{roleId}/reset", HandleAsync)
            .Sends<ResetPreferenceLayerCommand>()
            .WithSummary("删除偏好在一个角色上的值")
            .WithDescription("只删这个角色这一层，系统层的值或默认值重新对这个角色下没有自己设置过的人生效。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, [Description("角色 ID")] RoleId roleId, ResetPreferenceLayerRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetPreferenceLayerCommand(key, roleId, request.ExpectedVersion), ct)).ToOk();
}

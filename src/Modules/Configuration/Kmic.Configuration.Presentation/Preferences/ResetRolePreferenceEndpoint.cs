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
            .WithSummary("移除角色级偏好设置")
            .WithDescription("移除指定角色的偏好设置；存在有效个人设置时仍优先个人设置，否则顺延取系统层或默认值");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, [Description("角色 ID")] RoleId roleId, ResetPreferenceLayerRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetPreferenceLayerCommand(key, roleId, request.ExpectedVersion), ct)).ToOk();
}

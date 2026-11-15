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
            .WithDescription("仅移除指定角色的设置。允许且有效的个人设置仍优先，否则取允许的系统层有效值。均无可用值时，普通偏好使用代码默认值，个人偏好查询不再返回该偏好族键。必须提供幂等键。");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, [Description("角色 ID")] RoleId roleId, ResetPreferenceLayerRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetPreferenceLayerCommand(key, roleId, request.ExpectedVersion), ct)).ToOk();
}

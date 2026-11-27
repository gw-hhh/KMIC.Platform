using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Preferences.Mine;

namespace Kmic.Configuration.Presentation.Preferences;

internal sealed class ResetMyPreferenceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/preferences/me/{key}/reset", HandleAsync)
            .Sends<ResetMyPreferenceCommand>()
            .WithSummary("重置个人偏好")
            .WithDescription("移除当前角色下的个人偏好并按角色、系统、默认值顺延生效，无个人设置时不改动；只读模拟会话不可操作");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetMyPreferenceCommand(key), ct)).ToNoContent();
}

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
            .WithSummary("恢复本人的一个偏好")
            .WithDescription("删除本人这一层，值回落到角色层，再回落到系统层和默认值。本来就没有设置过时什么也不做。模拟登录时不能操作。");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetMyPreferenceCommand(key), ct)).ToNoContent();
}

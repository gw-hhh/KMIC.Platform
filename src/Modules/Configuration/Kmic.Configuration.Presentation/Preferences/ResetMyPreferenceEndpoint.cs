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
            .WithDescription("移除当前角色下的个人设置。之后按声明允许的角色层、系统层取有效值，均无可用值时使用代码默认值。未保存个人设置时不做修改；只读模拟会话不能操作。");

    private static async Task<IResult> HandleAsync([Description("偏好键")] string key, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResetMyPreferenceCommand(key), ct)).ToNoContent();
}

using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Preferences.Mine;

namespace Kmic.Configuration.Presentation.Preferences;

internal sealed class SetMyPreferenceEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/preferences/me/{key}", HandleAsync)
            .Sends<SetMyPreferenceCommand>()
            .WithSummary("保存个人偏好")
            .WithDescription("保存当前角色下的个人偏好并覆盖旧值；偏好族值须为符合大小上限的 JSON 对象，只读模拟会话不可操作");

    private static async Task<IResult> HandleAsync([Description("偏好键，如 Ui.Shell 或 Ui.Table.Sample.Specimens.Main")] string key, SetMyPreferenceRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetMyPreferenceCommand(key, request.Value), ct)).ToNoContent();
}

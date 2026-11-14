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
            .WithDescription("仅保存当前角色下的个人设置，再次保存同一键时覆盖原值。偏好族的键须符合格式要求，值须为大小符合限制的 JSON 对象，具体内容由前端约定。只读模拟会话不能保存。");

    private static async Task<IResult> HandleAsync([Description("偏好键，如 Ui.Shell 或 Ui.Table.Sample.Specimens.Main")] string key, SetMyPreferenceRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetMyPreferenceCommand(key, request.Value), ct)).ToNoContent();
}

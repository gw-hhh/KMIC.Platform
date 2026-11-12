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
            .WithSummary("保存本人的一个偏好")
            .WithDescription("只存本人在当前角色下的这一份，换了角色是另一份。偏好族里的键只校验格式、必须是 JSON 对象且不超大小，内容由前端自己约定。模拟登录时不能保存。同一个键再写一次是覆盖。");

    private static async Task<IResult> HandleAsync([Description("偏好键，如 Ui.Shell 或 Ui.Table.Sample.Specimens.Main")] string key, SetMyPreferenceRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SetMyPreferenceCommand(key, request.Value), ct)).ToNoContent();
}

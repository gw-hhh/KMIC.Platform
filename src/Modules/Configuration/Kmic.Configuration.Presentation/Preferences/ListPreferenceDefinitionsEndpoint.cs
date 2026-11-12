using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Preferences.Layers;

namespace Kmic.Configuration.Presentation.Preferences;

internal sealed class ListPreferenceDefinitionsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/preferences", HandleAsync)
            .Sends<ListPreferenceDefinitionsQuery>()
            .WithSummary("查询偏好清单")
            .WithDescription("管理页用：目录里声明的偏好和偏好族里已经有系统层或角色层值的键，各自允许的层、默认值、系统层和各角色层现有的值与版本。本人那一层不在这里。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListPreferenceDefinitionsQuery(), ct)).ToOk();
}

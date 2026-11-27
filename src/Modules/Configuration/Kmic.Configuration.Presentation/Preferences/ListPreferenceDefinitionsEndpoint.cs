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
            .WithDescription("返回偏好定义、默认值及系统/角色层的当前值与版本号（不含个人设置）");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListPreferenceDefinitionsQuery(), ct)).ToOk();
}

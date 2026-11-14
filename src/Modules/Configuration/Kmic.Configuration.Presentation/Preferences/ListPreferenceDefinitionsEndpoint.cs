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
            .WithDescription("供管理页使用，返回偏好定义及系统、角色层的设置值和版本，包括允许设置的层、代码默认值，以及已有系统或角色设置的偏好族键。不包含个人设置。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListPreferenceDefinitionsQuery(), ct)).ToOk();
}

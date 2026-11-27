using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Preferences.Mine;

namespace Kmic.Configuration.Presentation.Preferences;

internal sealed class GetMyPreferencesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/preferences/me", HandleAsync)
            .Sends<GetMyPreferencesQuery>()
            .WithSummary("查询个人偏好")
            .WithDescription("返回当前角色下生效的偏好值及来源（默认、系统、角色或个人，含已有值的偏好族键）");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetMyPreferencesQuery(), ct)).ToOk();
}

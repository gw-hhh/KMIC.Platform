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
            .WithSummary("查询本人的偏好")
            .WithDescription("当前用户在当前角色下全部偏好的生效值，每项带着值来自哪一层（默认、系统、角色、本人）。包括偏好族里已经有值的键。前端启动时拉一次，修改后更新本地。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetMyPreferencesQuery(), ct)).ToOk();
}

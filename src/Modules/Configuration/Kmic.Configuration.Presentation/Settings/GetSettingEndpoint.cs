using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Settings.Get;

namespace Kmic.Configuration.Presentation.Settings;

internal sealed class GetSettingEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/settings/{key}", HandleAsync)
            .Sends<GetSettingQuery>()
            .WithSummary("查询一个系统设置");

    private static async Task<IResult> HandleAsync([Description("设置键，如 Sample.Submission")] string key, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetSettingQuery(key), ct)).ToOk();
}

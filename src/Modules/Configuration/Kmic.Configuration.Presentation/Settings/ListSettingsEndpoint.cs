using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Settings.Get;

namespace Kmic.Configuration.Presentation.Settings;

internal sealed class ListSettingsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/settings", HandleAsync)
            .Sends<ListSettingsQuery>()
            .WithSummary("查询全部系统设置")
            .WithDescription("返回已声明设置的定义、当前值与当前用户可用操作，并单列已移除声明但仍存值的设置");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListSettingsQuery(), ct)).ToOk();
}

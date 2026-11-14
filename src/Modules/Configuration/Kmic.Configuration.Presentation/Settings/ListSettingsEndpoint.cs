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
            .WithDescription("返回已声明设置的字段、默认值、当前值、修改状态、修改权限和当前用户的可用操作，并单独列出已移除声明但仍保留值的设置。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListSettingsQuery(), ct)).ToOk();
}

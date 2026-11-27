using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Settings.Changes;

namespace Kmic.Configuration.Presentation.Settings;

internal sealed class ListSettingChangesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/setting-changes", HandleAsync)
            .Sends<ListSettingChangesQuery>()
            .Sortable(SettingChangeSort.Map)
            .WithSummary("查询系统设置的变更历史")
            .WithDescription("分页查询修改人、时间、原因及新旧值（默认时间倒序）；设置中不得存放密钥");

    private static async Task<IResult> HandleAsync([AsParameters] ListSettingChangesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListSettingChangesQuery(request.Key, request.From, request.To, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)), ct)).ToOk();
}

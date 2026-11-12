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
            .WithDescription("谁在什么时候把什么改成了什么，为什么。新旧值原样保存，所以设置里不放密钥。默认按时间倒序，分页。");

    private static async Task<IResult> HandleAsync([AsParameters] ListSettingChangesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListSettingChangesQuery(request.Key, request.From, request.To, new PageRequest(request.Page, request.PageSize), new SortRequest(request.Sort)), ct)).ToOk();
}

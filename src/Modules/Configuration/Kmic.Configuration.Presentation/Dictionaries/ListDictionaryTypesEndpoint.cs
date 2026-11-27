using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Dictionaries.Admin;

namespace Kmic.Configuration.Presentation.Dictionaries;

internal sealed class ListDictionaryTypesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/dictionary-types", HandleAsync)
            .Sends<ListDictionaryTypesQuery>()
            .WithSummary("查询全部字典类型")
            .WithDescription("返回字典类型、所属模块、项数与状态；类型由代码声明，仅维护字典项");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListDictionaryTypesQuery(), ct)).ToOk();
}

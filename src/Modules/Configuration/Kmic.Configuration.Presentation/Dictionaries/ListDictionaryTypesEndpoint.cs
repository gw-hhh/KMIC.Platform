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
            .WithDescription("类型、所属模块、字典项数、状态。字典类型由代码声明，这里只能维护它们的字典项。");

    private static async Task<IResult> HandleAsync(IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ListDictionaryTypesQuery(), ct)).ToOk();
}

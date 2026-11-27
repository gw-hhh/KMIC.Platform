using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Dictionaries.Get;

namespace Kmic.Configuration.Presentation.Dictionaries;

internal sealed class GetDictionariesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/dictionaries", HandleAsync)
            .Sends<GetDictionariesQuery>()
            .WithSummary("批量查询字典")
            .WithDescription("按逗号分隔的 types 批量查询（最多 20 个，忽略不存在的类型，含停用项）；支持 If-None-Match 命中 ETag 时返回 304");

    private static async Task<IResult> HandleAsync([Description("逗号分隔的字典类型编码，最多 20 个")] string types, HttpContext http, IQuerySender sender, CancellationToken ct)
    {
        var codes = types.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return (await sender.SendAsync(new GetDictionariesQuery(codes), ct)).ToOkWithETag(http, response => response.ETag);
    }
}

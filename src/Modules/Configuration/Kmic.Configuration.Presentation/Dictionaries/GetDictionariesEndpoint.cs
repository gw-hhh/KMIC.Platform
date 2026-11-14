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
            .WithDescription("types 为逗号分隔的类型编码，每次最多 20 个。登录即可查询，结果包含停用项及状态，不存在的类型会被忽略。响应包含 ETag；携带 If-None-Match 再次请求时，版本未变则返回 304。");

    private static async Task<IResult> HandleAsync([Description("字典类型编码，逗号分隔，一次最多 20 个")] string types, HttpContext http, IQuerySender sender, CancellationToken ct)
    {
        var codes = types.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return (await sender.SendAsync(new GetDictionariesQuery(codes), ct)).ToOkWithETag(http, response => response.ETag);
    }
}

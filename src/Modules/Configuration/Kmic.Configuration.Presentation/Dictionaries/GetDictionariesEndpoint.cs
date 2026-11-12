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
            .WithSummary("取一批字典")
            .WithDescription("types 是逗号分隔的字典类型编码，一次最多 20 个。登录即可，含停用的项并带状态；不存在的类型不出现在结果里。响应带 ETag（各类型版本的组合），前端带 If-None-Match 再问，没变就返回 304。");

    private static async Task<IResult> HandleAsync([Description("字典类型编码，逗号分隔，一次最多 20 个")] string types, HttpContext http, IQuerySender sender, CancellationToken ct)
    {
        var codes = types.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        return (await sender.SendAsync(new GetDictionariesQuery(codes), ct)).ToOkWithETag(http, response => response.ETag);
    }
}

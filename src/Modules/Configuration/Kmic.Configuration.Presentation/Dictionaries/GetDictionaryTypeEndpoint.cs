using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Dictionaries.Admin;

namespace Kmic.Configuration.Presentation.Dictionaries;

internal sealed class GetDictionaryTypeEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/dictionary-types/{typeCode}", HandleAsync)
            .Sends<GetDictionaryTypeQuery>()
            .WithSummary("查询字典类型及全部字典项")
            .WithDescription("返回字典项（含停用项）与当前版本号，修改字典项时须回传该 version");

    private static async Task<IResult> HandleAsync([Description("字典类型编码，如 Sample.Industry")] string typeCode, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetDictionaryTypeQuery(typeCode), ct)).ToOk();
}

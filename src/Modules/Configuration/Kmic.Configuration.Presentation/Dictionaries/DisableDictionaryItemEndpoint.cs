using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Dictionaries.Items;

namespace Kmic.Configuration.Presentation.Dictionaries;

internal sealed class DisableDictionaryItemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/dictionary-types/{typeCode}/items/{code}/disable", HandleAsync)
            .Sends<DisableDictionaryItemCommand>()
            .WithSummary("停用字典项")
            .WithDescription("历史数据仍然显示“已停用”，新数据不能再选。下面有启用的子项时返回 422，先停用子项。");

    private static async Task<IResult> HandleAsync([Description("字典类型编码")] string typeCode, [Description("字典项编码")] string code, DictionaryItemVersionRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new DisableDictionaryItemCommand(typeCode, request.ExpectedVersion, code), ct)).ToOk();
}

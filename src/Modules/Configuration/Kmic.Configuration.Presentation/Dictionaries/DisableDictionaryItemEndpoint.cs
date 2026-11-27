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
            .WithDescription("停用后仅历史数据可见，新数据不可选；若仍有启用的子项则返回 422");

    private static async Task<IResult> HandleAsync([Description("字典类型编码")] string typeCode, [Description("字典项编码")] string code, DictionaryItemVersionRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new DisableDictionaryItemCommand(typeCode, request.ExpectedVersion, code), ct)).ToOk();
}

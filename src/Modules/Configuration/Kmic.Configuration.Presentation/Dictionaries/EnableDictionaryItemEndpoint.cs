using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Dictionaries.Items;

namespace Kmic.Configuration.Presentation.Dictionaries;

internal sealed class EnableDictionaryItemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/dictionary-types/{typeCode}/items/{code}/enable", HandleAsync)
            .Sends<EnableDictionaryItemCommand>()
            .WithSummary("启用字典项")
            .WithDescription("它的父项必须是启用的。");

    private static async Task<IResult> HandleAsync([Description("字典类型编码")] string typeCode, [Description("字典项编码")] string code, DictionaryItemVersionRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new EnableDictionaryItemCommand(typeCode, request.ExpectedVersion, code), ct)).ToOk();
}

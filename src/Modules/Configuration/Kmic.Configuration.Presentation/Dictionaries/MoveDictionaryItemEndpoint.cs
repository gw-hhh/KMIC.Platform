using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Dictionaries.Items;

namespace Kmic.Configuration.Presentation.Dictionaries;

internal sealed class MoveDictionaryItemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/dictionary-types/{typeCode}/items/{code}/move", HandleAsync)
            .Sends<MoveDictionaryItemCommand>()
            .WithSummary("移动字典项")
            .WithDescription("只对层级字典；排在新父项下的最后。");

    private static async Task<IResult> HandleAsync([Description("字典类型编码")] string typeCode, [Description("字典项编码")] string code, MoveDictionaryItemRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new MoveDictionaryItemCommand(typeCode, request.ExpectedVersion, code, request.NewParentCode), ct)).ToOk();
}

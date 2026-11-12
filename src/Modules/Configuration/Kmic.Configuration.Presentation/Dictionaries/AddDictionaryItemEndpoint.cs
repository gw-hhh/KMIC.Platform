using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Dictionaries.Items;

namespace Kmic.Configuration.Presentation.Dictionaries;

internal sealed class AddDictionaryItemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/dictionary-types/{typeCode}/items", HandleAsync)
            .Sends<AddDictionaryItemCommand>()
            .WithSummary("添加字典项")
            .WithDescription("编码在同一类型内唯一（不区分大小写），创建后不能改。不分层的字典不能给父项。必须带幂等键。");

    private static async Task<IResult> HandleAsync([Description("字典类型编码")] string typeCode, AddDictionaryItemRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AddDictionaryItemCommand(typeCode, request.ExpectedVersion, request.Code, request.Name, request.ParentCode, request.SortOrder), ct)).ToOk();
}

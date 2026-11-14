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
            .WithDescription("同一类型内的编码唯一，不区分大小写，创建后不可修改。非层级字典不能指定父项。必须提供幂等键。");

    private static async Task<IResult> HandleAsync([Description("字典类型编码")] string typeCode, AddDictionaryItemRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AddDictionaryItemCommand(typeCode, request.ExpectedVersion, request.Code, request.Name, request.ParentCode, request.SortOrder), ct)).ToOk();
}

using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Dictionaries.Items;

namespace Kmic.Configuration.Presentation.Dictionaries;

internal sealed class ReorderDictionaryItemsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/dictionary-types/{typeCode}/items-order", HandleAsync)
            .Sends<ReorderDictionaryItemsCommand>()
            .WithSummary("调整同级字典项顺序")
            .WithDescription("必须提交同一父项下的全部字典项，不能缺少、增加或重复，避免遗漏并发新增的项。");

    private static async Task<IResult> HandleAsync([Description("字典类型编码")] string typeCode, ReorderDictionaryItemsRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ReorderDictionaryItemsCommand(typeCode, request.ExpectedVersion, request.ParentCode, request.OrderedCodes), ct)).ToOk();
}

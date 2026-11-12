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
            .WithSummary("调整同一父项下全部字典项的次序")
            .WithDescription("整体提交：必须包含该父项下的全部字典项，缺项或多项都拒绝，避免别人同时添加的项被丢掉。");

    private static async Task<IResult> HandleAsync([Description("字典类型编码")] string typeCode, ReorderDictionaryItemsRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ReorderDictionaryItemsCommand(typeCode, request.ExpectedVersion, request.ParentCode, request.OrderedCodes), ct)).ToOk();
}

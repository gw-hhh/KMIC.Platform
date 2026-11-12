using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Configuration.Application.Dictionaries.Items;

namespace Kmic.Configuration.Presentation.Dictionaries;

internal sealed class RenameDictionaryItemEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/dictionary-types/{typeCode}/items/{code}", HandleAsync)
            .Sends<RenameDictionaryItemCommand>()
            .WithSummary("修改字典项的显示名")
            .WithDescription("只改显示名，编码不能改：业务数据里存的是编码。");

    private static async Task<IResult> HandleAsync([Description("字典类型编码")] string typeCode, [Description("字典项编码")] string code, RenameDictionaryItemRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RenameDictionaryItemCommand(typeCode, request.ExpectedVersion, code, request.Name), ct)).ToOk();
}

using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Specimens.Close;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Presentation.Specimens;

internal sealed class CloseSpecimenEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/specimens/{id}/close", HandleAsync)
            .Sends<CloseSpecimenCommand>()
            .WithSummary("关闭示例标本")
            .WithDescription("已提交的标本关闭。只有负责人可以关闭。");

    private static async Task<IResult> HandleAsync([Description("标本 ID")] SpecimenId id, CloseSpecimenRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new CloseSpecimenCommand(id, request.ExpectedVersion), ct)).ToOk();
}

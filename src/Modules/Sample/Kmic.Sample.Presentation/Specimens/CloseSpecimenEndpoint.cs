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
            .WithSummary("关闭示例")
            .WithDescription("只有负责人可以关闭已提交的示例。");

    private static async Task<IResult> HandleAsync([Description("示例 ID")] SpecimenId id, CloseSpecimenRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new CloseSpecimenCommand(id, request.ExpectedVersion), ct)).ToOk();
}

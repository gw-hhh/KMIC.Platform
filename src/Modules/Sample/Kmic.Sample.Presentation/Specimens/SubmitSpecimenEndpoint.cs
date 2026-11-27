using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Specimens.Submit;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Presentation.Specimens;

internal sealed class SubmitSpecimenEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/specimens/{id}/submit", HandleAsync)
            .Sends<SubmitSpecimenCommand>()
            .WithSummary("提交示例")
            .WithDescription("仅负责人可提交含至少一项明细的草稿");

    private static async Task<IResult> HandleAsync([Description("示例 ID")] SpecimenId id, SubmitSpecimenRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SubmitSpecimenCommand(id, request.ExpectedVersion), ct)).ToOk();
}

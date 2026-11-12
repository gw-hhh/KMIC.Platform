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
            .WithSummary("提交示例标本")
            .WithDescription("草稿提交为已提交。只有负责人可以提交，至少要有一项明细。");

    private static async Task<IResult> HandleAsync([Description("标本 ID")] SpecimenId id, SubmitSpecimenRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new SubmitSpecimenCommand(id, request.ExpectedVersion), ct)).ToOk();
}

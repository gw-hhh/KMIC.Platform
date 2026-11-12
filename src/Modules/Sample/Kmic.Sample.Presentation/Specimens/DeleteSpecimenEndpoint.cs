using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Specimens.Delete;
using Kmic.Sample.Domain.Specimens;
using Microsoft.AspNetCore.Mvc;

namespace Kmic.Sample.Presentation.Specimens;

internal sealed class DeleteSpecimenEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/specimens/{id}", HandleAsync)
            .Sends<DeleteSpecimenCommand>()
            .WithSummary("删除示例标本")
            .WithDescription("只能删除草稿，软删除。成功返回 204。");

    private static async Task<IResult> HandleAsync([Description("标本 ID")] SpecimenId id, [FromBody] DeleteSpecimenRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new DeleteSpecimenCommand(id, request.ExpectedVersion), ct)).ToNoContent();
}

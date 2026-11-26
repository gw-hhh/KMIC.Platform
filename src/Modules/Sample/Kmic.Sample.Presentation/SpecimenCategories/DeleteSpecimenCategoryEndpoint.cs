using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Maintenance.SpecimenCategories;
using Kmic.Sample.Domain.SpecimenCategories;
using Microsoft.AspNetCore.Mvc;

namespace Kmic.Sample.Presentation.SpecimenCategories;

internal sealed class DeleteSpecimenCategoryEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/specimen-categories/{id}", HandleAsync)
            .Sends<DeleteSpecimenCategoryCommand>()
            .WithSummary("删除示例分类");

    private static async Task<IResult> HandleAsync(
        [Description("示例分类 ID")] SpecimenCategoryId id, [FromBody] DeleteSpecimenCategoryRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new DeleteSpecimenCategoryCommand(id, request.ExpectedVersion), ct)).ToNoContent();
}

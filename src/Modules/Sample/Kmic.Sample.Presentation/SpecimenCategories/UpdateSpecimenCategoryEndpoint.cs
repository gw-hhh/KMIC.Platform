using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Maintenance.SpecimenCategories;
using Kmic.Sample.Domain.SpecimenCategories;

namespace Kmic.Sample.Presentation.SpecimenCategories;

internal sealed class UpdateSpecimenCategoryEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/specimen-categories/{id}", HandleAsync)
            .Sends<UpdateSpecimenCategoryCommand>()
            .WithSummary("修改示例分类")
            .WithDescription("修改名称和启用状态，重名返回 409。");

    private static async Task<IResult> HandleAsync(
        [Description("示例分类 ID")] SpecimenCategoryId id, UpdateSpecimenCategoryRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new UpdateSpecimenCategoryCommand(id, request.Name, request.IsEnabled, request.ExpectedVersion), ct)).ToOk();
}

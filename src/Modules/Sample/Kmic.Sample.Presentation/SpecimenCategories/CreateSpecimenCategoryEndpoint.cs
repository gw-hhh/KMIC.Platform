using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Maintenance.SpecimenCategories;

namespace Kmic.Sample.Presentation.SpecimenCategories;

internal sealed class CreateSpecimenCategoryEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/specimen-categories", HandleAsync)
            .Sends<CreateSpecimenCategoryCommand>()
            .WithSummary("新建示例分类")
            .WithDescription("名称不区分大小写，重名返回 409。必须提供幂等键。成功返回 201，Location 指向新分类。");

    private static async Task<IResult> HandleAsync(CreateSpecimenCategoryRequest request, ICommandSender sender, CancellationToken ct)
    {
        var result = await sender.SendAsync(new CreateSpecimenCategoryCommand(request.Name), ct);
        return result.ToCreated(id => $"/api/v1/sample/specimen-categories/{id.Value}");
    }
}

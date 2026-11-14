using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Specimens.Create;

namespace Kmic.Sample.Presentation.Specimens;

internal sealed class CreateSpecimenEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/specimens", HandleAsync)
            .Sends<CreateSpecimenCommand>()
            .WithSummary("创建示例")
            .WithDescription("创建草稿，负责人为当前用户。必须提供幂等键，超时重试时使用同一个键。成功返回 201，Location 指向新建示例的地址。");

    private static async Task<IResult> HandleAsync(CreateSpecimenRequest request, ICommandSender sender, CancellationToken ct)
    {
        var command = new CreateSpecimenCommand(request.Title, [.. request.Items.Select(i => new CreateSpecimenItemInput(i.Name, i.Quantity))]);
        var result = await sender.SendAsync(command, ct);
        return result.ToCreated(id => $"/api/v1/sample/specimens/{id.Value}");
    }
}

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
            .WithSummary("创建示例标本")
            .WithDescription("创建一个草稿标本，负责人是当前用户。必须带幂等键，超时重试用同一个键。成功返回 201，Location 指向新标本。");

    private static async Task<IResult> HandleAsync(CreateSpecimenRequest request, ICommandSender sender, CancellationToken ct)
    {
        var command = new CreateSpecimenCommand(request.Title, [.. request.Items.Select(i => new CreateSpecimenItemInput(i.Name, i.Quantity))]);
        var result = await sender.SendAsync(command, ct);
        return result.ToCreated(id => $"/api/v1/sample/specimens/{id.Value}");
    }
}

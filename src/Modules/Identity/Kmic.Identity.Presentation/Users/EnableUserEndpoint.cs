using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.Enable;

namespace Kmic.Identity.Presentation.Users;

internal sealed class EnableUserEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/users/{id}/enable", HandleAsync)
            .Sends<EnableUserCommand>()
            .WithSummary("启用用户");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, EnableUserRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new EnableUserCommand(id, request.ExpectedVersion), ct)).ToOk();
}

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.Create;

namespace Kmic.Identity.Presentation.Users;

internal sealed class CreateUserEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/users", HandleAsync)
            .Sends<CreateUserCommand>()
            .WithSummary("新建用户")
            .WithDescription("创建时不设置密码、所属组织或角色，需通过对应接口配置。账号、手机号或工号重复时返回对应的 409 错误。必须提供幂等键。");

    private static async Task<IResult> HandleAsync(CreateUserRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new CreateUserCommand(request.Account, request.DisplayName, request.EnglishName, request.EmployeeNo, request.Mobile, request.Email), ct))
            .ToCreated(id => $"/api/v1/identity/users/{id.Value}");
}

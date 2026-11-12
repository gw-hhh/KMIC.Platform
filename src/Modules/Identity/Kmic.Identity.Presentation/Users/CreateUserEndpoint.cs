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
            .WithDescription("不设置密码、所属组织和角色，这些用各自的接口设置。账号、手机号、工号重复分别返回对应的 409。必须带幂等键。");

    private static async Task<IResult> HandleAsync(CreateUserRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new CreateUserCommand(request.Account, request.DisplayName, request.EnglishName, request.EmployeeNo, request.Mobile, request.Email), ct))
            .ToCreated(id => $"/api/v1/identity/users/{id.Value}");
}

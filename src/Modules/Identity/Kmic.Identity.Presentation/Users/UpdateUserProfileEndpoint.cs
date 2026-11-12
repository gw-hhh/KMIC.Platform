using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Identity.Application.Users.UpdateProfile;

namespace Kmic.Identity.Presentation.Users;

internal sealed class UpdateUserProfileEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPut("/users/{id}", HandleAsync)
            .Sends<UpdateUserProfileCommand>()
            .WithSummary("修改用户资料")
            .WithDescription("整体提交资料字段。账号和手机号是登录凭据，对持有受保护角色的用户，只有同样持有受保护角色的人才能改这两项。");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, UpdateUserProfileRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new UpdateUserProfileCommand(id, request.ExpectedVersion, request.Account, request.DisplayName, request.EnglishName, request.EmployeeNo, request.Mobile, request.Email), ct)).ToOk();
}

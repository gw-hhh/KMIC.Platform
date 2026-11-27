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
            .WithDescription("全量覆盖资料字段；修改账号或手机号时，若目标持受保护角色则操作者当前角色也须受保护");

    private static async Task<IResult> HandleAsync([Description("用户 ID")] UserId id, UpdateUserProfileRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new UpdateUserProfileCommand(id, request.ExpectedVersion, request.Account, request.DisplayName, request.EnglishName, request.EmployeeNo, request.Mobile, request.Email), ct)).ToOk();
}

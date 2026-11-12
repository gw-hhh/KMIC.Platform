using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Users;

namespace Kmic.Identity.Application.Users.Create;

/// <summary>
/// 处理 <see cref="CreateUserCommand"/>
/// </summary>
public sealed class CreateUserCommandHandler(IUserRepository users, IOperatorRecheck recheck, IExecutionContext execution)
    : AuthorizationAdminCommandHandler<CreateUserCommand, CreatedResponse<UserId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override ValueTask<Result<CreatedResponse<UserId>>> ExecuteAsync(CreateUserCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var created = User.Create(command.Account, command.DisplayName, command.EnglishName, command.EmployeeNo, command.Mobile, command.Email);
        if (created.IsFailure)
        {
            return ValueTask.FromResult<Result<CreatedResponse<UserId>>>(created.Error);
        }

        users.Add(created.Value);
        return ValueTask.FromResult<Result<CreatedResponse<UserId>>>(new CreatedResponse<UserId>(created.Value.Id));
    }
}

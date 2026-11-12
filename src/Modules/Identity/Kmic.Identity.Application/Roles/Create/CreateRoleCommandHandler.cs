using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Identity.Application.Authorization;
using Kmic.Identity.Domain.Roles;

namespace Kmic.Identity.Application.Roles.Create;

/// <summary>
/// 处理 <see cref="CreateRoleCommand"/>
/// </summary>
public sealed class CreateRoleCommandHandler(IRoleRepository roles, IOperatorRecheck recheck, IExecutionContext execution)
    : AuthorizationAdminCommandHandler<CreateRoleCommand, CreatedResponse<RoleId>>(recheck, execution)
{
    /// <inheritdoc />
    protected override ValueTask<Result<CreatedResponse<RoleId>>> ExecuteAsync(CreateRoleCommand command, IAccessContext access, CancellationToken cancellationToken)
    {
        var created = Role.Create(command.Name, command.Description, command.SortOrder, command.DefaultScopeLevel);
        if (created.IsFailure)
        {
            return ValueTask.FromResult<Result<CreatedResponse<RoleId>>>(created.Error);
        }

        roles.Add(created.Value);
        return ValueTask.FromResult<Result<CreatedResponse<RoleId>>>(new CreatedResponse<RoleId>(created.Value.Id));
    }
}

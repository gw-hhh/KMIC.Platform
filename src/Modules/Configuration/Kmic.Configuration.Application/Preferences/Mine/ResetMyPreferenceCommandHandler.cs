using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Preferences;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 移除当前角色下的个人偏好设置
/// </summary>
public sealed class ResetMyPreferenceCommandHandler(
    PreferenceCatalog catalog,
    IPreferenceRepository preferences,
    IExecutionContext execution) : ICommandHandler<ResetMyPreferenceCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(ResetMyPreferenceCommand command, CancellationToken cancellationToken)
    {
        var found = PreferenceSubject.Find(catalog, command.Key);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var access = execution.Access ?? throw new InvalidOperationException("恢复个人偏好需要已登录的访问上下文。");
        var row = await preferences.FindForUpdateAsync(found.Value.Key, PreferenceLayer.UserRole, access.RoleId, access.UserId, cancellationToken);
        if (row is not null)
        {
            preferences.Remove(row);
        }

        return Result.Success();
    }
}

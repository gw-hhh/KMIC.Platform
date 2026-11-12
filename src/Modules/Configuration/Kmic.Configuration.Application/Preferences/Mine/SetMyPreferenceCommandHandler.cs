using Kmic.BuildingBlocks.Application.Execution;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Preferences;
using Microsoft.Extensions.Options;

namespace Kmic.Configuration.Application.Preferences.Mine;

/// <summary>
/// 处理 <see cref="SetMyPreferenceCommand"/>
/// </summary>
public sealed class SetMyPreferenceCommandHandler(
    PreferenceCatalog catalog,
    IPreferenceRepository preferences,
    IExecutionContext execution,
    IOptions<PreferenceOptions> options) : ICommandHandler<SetMyPreferenceCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(SetMyPreferenceCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var found = PreferenceSubject.Find(catalog, command.Key);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var subject = found.Value;
        var allowed = subject.CheckLayer(PreferenceLayer.UserRole);
        if (allowed.IsFailure)
        {
            return allowed.Error;
        }

        var json = subject.Normalize(command.Value, options.Value.FamilyValueMaxKb);
        if (json.IsFailure)
        {
            return json.Error;
        }

        var access = execution.Access ?? throw new InvalidOperationException("保存个人偏好需要已登录的访问上下文。");
        var row = await preferences.FindForUpdateAsync(subject.Key, PreferenceLayer.UserRole, access.RoleId, access.UserId, cancellationToken);
        if (row is null)
        {
            preferences.Add(PreferenceValue.ForUserRole(subject.Key, access.RoleId, access.UserId, json.Value));
        }
        else
        {
            row.Set(json.Value);
        }

        return Result.Success();
    }
}

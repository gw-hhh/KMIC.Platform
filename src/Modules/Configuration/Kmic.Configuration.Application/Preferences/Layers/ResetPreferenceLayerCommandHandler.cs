using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Identity.Contracts;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 处理 <see cref="ResetPreferenceLayerCommand"/>
/// </summary>
public sealed class ResetPreferenceLayerCommandHandler(
    PreferenceCatalog catalog,
    IPreferenceRepository preferences,
    IRoleReferences roles,
    IAfterCommit afterCommit) : ICommandHandler<ResetPreferenceLayerCommand, VersionedResponse<PreferenceValueId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<PreferenceValueId>>> Handle(ResetPreferenceLayerCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var found = PreferenceSubject.Find(catalog, command.Key);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var resolved = await PreferenceLayerTarget.ResolveAsync(command.RoleId, found.Value, roles, cancellationToken);
        if (resolved.IsFailure)
        {
            return resolved.Error;
        }

        var target = resolved.Value;
        var row = await preferences.FindForUpdateAsync(found.Value.Key, target.Layer, target.RoleId, null, cancellationToken);
        if (row is null)
        {
            // 调用方看到过这一层的值，现在已经没有了：别人在这期间删除了它
            return CommonErrors.VersionConflict;
        }

        var versionOk = PreferenceLayerTarget.CheckVersion(row, command.ExpectedVersion);
        if (versionOk.IsFailure)
        {
            return versionOk.Error;
        }

        preferences.Remove(row);
        afterCommit.InvalidateTags(target.CacheTag);
        return new VersionedResponse<PreferenceValueId>(row.Id);
    }
}

using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Identity.Contracts;
using Microsoft.Extensions.Options;

namespace Kmic.Configuration.Application.Preferences.Layers;

/// <summary>
/// 处理 <see cref="SetPreferenceLayerCommand"/>
/// </summary>
public sealed class SetPreferenceLayerCommandHandler(
    PreferenceCatalog catalog,
    IPreferenceRepository preferences,
    IRoleReferences roles,
    IOptions<PreferenceOptions> options,
    IAfterCommit afterCommit) : ICommandHandler<SetPreferenceLayerCommand, VersionedResponse<PreferenceValueId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<PreferenceValueId>>> Handle(SetPreferenceLayerCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var found = PreferenceSubject.Find(catalog, command.Key);
        if (found.IsFailure)
        {
            return found.Error;
        }

        var subject = found.Value;
        var resolved = await PreferenceLayerTarget.ResolveAsync(command.RoleId, subject, roles, cancellationToken);
        if (resolved.IsFailure)
        {
            return resolved.Error;
        }

        var target = resolved.Value;
        var json = subject.Normalize(command.Value, options.Value.FamilyValueMaxKb);
        if (json.IsFailure)
        {
            return json.Error;
        }

        var row = await preferences.FindForUpdateAsync(subject.Key, target.Layer, target.RoleId, null, cancellationToken);
        var versionOk = PreferenceLayerTarget.CheckVersion(row, command.ExpectedVersion);
        if (versionOk.IsFailure)
        {
            return versionOk.Error;
        }

        if (row is null)
        {
            row = target.RoleId is { } role
                ? PreferenceValue.ForRole(subject.Key, role, json.Value)
                : PreferenceValue.ForSystem(subject.Key, json.Value);
            preferences.Add(row);
        }
        else
        {
            row.Set(json.Value);
        }

        afterCommit.InvalidateTags(target.CacheTag);
        return new VersionedResponse<PreferenceValueId>(row.Id);
    }
}

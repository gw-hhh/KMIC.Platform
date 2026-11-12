using System.Diagnostics;
using System.Text;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Settings;
using Microsoft.Extensions.Options;

namespace Kmic.Configuration.Application.Settings.Set;

/// <summary>
/// 处理 <see cref="SetSettingCommand"/>
/// </summary>
public sealed class SetSettingCommandHandler(
    SystemSettingCatalog catalog,
    ISystemSettingRepository settings,
    IResourceAuthorizer<SettingTarget, SettingAction> authorizer,
    IOptions<SettingOptions> options,
    IAfterCommit afterCommit) : ICommandHandler<SetSettingCommand, VersionedResponse<SystemSettingValueId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<SystemSettingValueId>>> Handle(SetSettingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var definition = catalog.Find(command.Key);
        if (definition is null)
        {
            return ConfigurationErrors.SettingNotFound;
        }

        var allowed = authorizer.Check(new SettingTarget(definition.Key, definition.EditPermission), SettingAction.Edit);
        if (allowed.IsFailure)
        {
            return allowed.Error;
        }

        var parsed = DeclaredValues.Parse(definition, command.Value, ConfigurationErrors.SettingInvalid, ConfigurationErrors.SettingInvalidWith);
        if (parsed.IsFailure)
        {
            return parsed.Error;
        }

        var json = DeclaredValues.ToJson(definition, parsed.Value);
        if (Encoding.UTF8.GetByteCount(json) > options.Value.ValueMaxKb * 1024)
        {
            return ConfigurationErrors.SettingInvalid.WithDetail($"值太大，序列化后不能超过 {options.Value.ValueMaxKb} KB");
        }

        var loaded = await settings.GetByKeyForUpdateAsync(definition.Key, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var row = loaded.Value;
        var oldValue = row?.Value;
        if (row is null)
        {
            row = SystemSettingValue.Create(definition.Key, json);
            settings.Add(row);
        }
        else
        {
            row.Set(json);
        }

        settings.AddChange(SystemSettingChange.Record(definition.Key, oldValue, json, command.Reason.Trim(), Activity.Current?.TraceId.ToString()));
        afterCommit.InvalidateTags(ConfigurationCacheKeys.SettingsTag);
        return new VersionedResponse<SystemSettingValueId>(row.Id);
    }
}

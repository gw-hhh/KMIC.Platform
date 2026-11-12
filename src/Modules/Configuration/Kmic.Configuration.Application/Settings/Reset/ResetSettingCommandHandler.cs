using System.Diagnostics;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Reset;

/// <summary>
/// 处理 <see cref="ResetSettingCommand"/>
/// </summary>
public sealed class ResetSettingCommandHandler(
    SystemSettingCatalog catalog,
    ISystemSettingRepository settings,
    IResourceAuthorizer<SettingTarget, SettingAction> authorizer,
    IAfterCommit afterCommit) : ICommandHandler<ResetSettingCommand, VersionedResponse<SystemSettingValueId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<SystemSettingValueId>>> Handle(ResetSettingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var definition = catalog.Find(command.Key);
        if (definition is null)
        {
            return ConfigurationErrors.SettingNotFound;
        }

        var allowed = authorizer.Check(new SettingTarget(definition.Key, definition.EditPermission), SettingAction.Reset);
        if (allowed.IsFailure)
        {
            return allowed.Error;
        }

        var loaded = await settings.GetByKeyForUpdateAsync(definition.Key, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        // 已经是默认值：没有什么可恢复的
        var row = loaded.Value ?? throw new InvalidOperationException("带着版本号恢复默认值，却找不到值行，仓储应该已经返回版本冲突。");
        settings.AddChange(SystemSettingChange.Record(definition.Key, row.Value, null, command.Reason.Trim(), Activity.Current?.TraceId.ToString()));
        settings.Remove(row);
        afterCommit.InvalidateTags(ConfigurationCacheKeys.SettingsTag);
        return new VersionedResponse<SystemSettingValueId>(row.Id);
    }
}

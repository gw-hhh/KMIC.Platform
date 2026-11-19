using System.Diagnostics;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Purge;

/// <summary>
/// 清除已移除声明的系统设置值
/// </summary>
public sealed class PurgeObsoleteSettingCommandHandler(SystemSettingCatalog catalog, ISystemSettingRepository settings, IAfterCommit afterCommit)
    : ICommandHandler<PurgeObsoleteSettingCommand, VersionedResponse<SystemSettingValueId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<SystemSettingValueId>>> Handle(PurgeObsoleteSettingCommand command, CancellationToken cancellationToken)
    {
        if (catalog.Find(command.Key) is not null)
        {
            return ConfigurationErrors.InvalidState.WithDetail("该设置仍有代码声明，不能清除；如需重置，请恢复默认值");
        }

        var loaded = await settings.GetByKeyForUpdateAsync(command.Key, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var row = loaded.Value ?? throw new InvalidOperationException("未找到待清除的设置值，仓储应返回版本冲突。");
        settings.AddChange(SystemSettingChange.Record(command.Key, row.Value, null, command.Reason.Trim(), Activity.Current?.TraceId.ToString()));
        settings.Remove(row);
        afterCommit.InvalidateTags(ConfigurationCacheKeys.SettingsTag);
        return new VersionedResponse<SystemSettingValueId>(row.Id);
    }
}

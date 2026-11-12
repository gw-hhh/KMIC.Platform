using System.Diagnostics;
using Kmic.BuildingBlocks.Application.Events;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Configuration.Domain.Errors;
using Kmic.Configuration.Domain.Settings;

namespace Kmic.Configuration.Application.Settings.Purge;

/// <summary>
/// 处理 <see cref="PurgeObsoleteSettingCommand"/>
/// </summary>
public sealed class PurgeObsoleteSettingCommandHandler(SystemSettingCatalog catalog, ISystemSettingRepository settings, IAfterCommit afterCommit)
    : ICommandHandler<PurgeObsoleteSettingCommand, VersionedResponse<SystemSettingValueId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<SystemSettingValueId>>> Handle(PurgeObsoleteSettingCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        if (catalog.Find(command.Key) is not null)
        {
            return ConfigurationErrors.InvalidState.WithDetail("这个设置在代码里还在用，不能清除，请用恢复默认");
        }

        var loaded = await settings.GetByKeyForUpdateAsync(command.Key, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var row = loaded.Value ?? throw new InvalidOperationException("带着版本号清除设置值，却找不到值行，仓储应该已经返回版本冲突。");
        settings.AddChange(SystemSettingChange.Record(command.Key, row.Value, null, command.Reason.Trim(), Activity.Current?.TraceId.ToString()));
        settings.Remove(row);
        afterCommit.InvalidateTags(ConfigurationCacheKeys.SettingsTag);
        return new VersionedResponse<SystemSettingValueId>(row.Id);
    }
}

using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Operations.Domain.Errors;
using Kmic.Operations.Domain.Jobs;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Infrastructure.Persistence.Repositories;

internal sealed class ScheduledJobSettingRepository(OperationsDbContext context)
    : EfRepository<OperationsDbContext, ScheduledJobSetting, ScheduledJobSettingId>(context), IScheduledJobSettingRepository
{
    protected override Error NotFound => OperationsErrors.JobNotFound;

    public async Task<Result<ScheduledJobSetting?>> GetByJobNameForUpdateAsync(string jobName, long? expectedVersion, CancellationToken ct)
    {
        var id = await Context.Set<ScheduledJobSetting>()
            .Where(s => s.JobName == jobName)
            .Select(s => (ScheduledJobSettingId?)s.Id)
            .SingleOrDefaultAsync(ct);
        if (id is null)
        {
            // 带版本号但设置记录已不存在，按版本冲突处理
            return expectedVersion is null ? (ScheduledJobSetting?)null : CommonErrors.VersionConflict;
        }

        // 未带版本号说明读取时无设置记录，若此时记录已存在则判定为版本冲突
        if (expectedVersion is null)
        {
            return CommonErrors.VersionConflict;
        }

        var loaded = await GetForUpdateAsync(id.Value, expectedVersion, ct);
        return loaded.IsFailure ? loaded.Error : loaded.Value;
    }
}

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
            // 调用方说它看到的是某个版本，设置行却已经没有了
            return expectedVersion is null ? (ScheduledJobSetting?)null : CommonErrors.VersionConflict;
        }

        // 调用方没给版本号，说明它看到的是没改过的状态，而现在已经有人改过了
        if (expectedVersion is null)
        {
            return CommonErrors.VersionConflict;
        }

        var loaded = await GetForUpdateAsync(id.Value, expectedVersion, ct);
        return loaded.IsFailure ? loaded.Error : loaded.Value;
    }
}

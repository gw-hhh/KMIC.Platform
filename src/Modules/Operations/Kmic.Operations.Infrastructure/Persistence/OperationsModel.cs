using Kmic.Operations.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Operations.Infrastructure.Persistence;

internal static class OperationsModel
{
    public static void Configure(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new AuditLogConfiguration());
        modelBuilder.ApplyConfiguration(new ScheduledJobSettingConfiguration());
    }
}

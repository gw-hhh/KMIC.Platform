using Kmic.Configuration.Infrastructure.Persistence.Configurations;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Infrastructure.Persistence;

internal static class ConfigurationModel
{
    public static void Configure(ModelBuilder modelBuilder) =>
        modelBuilder
            .ApplyConfiguration(new DictionaryTypeConfiguration())
            .ApplyConfiguration(new DictionaryItemConfiguration())
            .ApplyConfiguration(new SystemSettingValueConfiguration())
            .ApplyConfiguration(new SystemSettingChangeConfiguration())
            .ApplyConfiguration(new PreferenceValueConfiguration());
}

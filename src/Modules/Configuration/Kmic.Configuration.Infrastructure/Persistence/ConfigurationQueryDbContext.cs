using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Application;
using Kmic.Configuration.Contracts;
using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Configuration.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Infrastructure.Persistence;

internal sealed class ConfigurationQueryDbContext(DbContextOptions<ConfigurationQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), IConfigurationQueryDb
{
    public override string Module => ConfigurationModuleInfo.Name;

    public IQueryable<DictionaryType> DictionaryTypes => Set<DictionaryType>();

    public IQueryable<SystemSettingValue> SettingValues => Set<SystemSettingValue>();

    public IQueryable<SystemSettingChange> SettingChanges => Set<SystemSettingChange>();

    public IQueryable<PreferenceValue> PreferenceValues => Set<PreferenceValue>();

    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInConfigurationIdConverters();

    protected override void ConfigureModel(ModelBuilder modelBuilder) => ConfigurationModel.Configure(modelBuilder);
}

using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Application;
using Kmic.Configuration.Contracts;
using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Configuration.Domain.Settings;
using Microsoft.EntityFrameworkCore;

namespace Kmic.Configuration.Infrastructure.Persistence;

/// <summary>
/// Configuration 模块的查询上下文：不跟踪实体，查询处理器通过 IConfigurationQueryDb 使用
/// </summary>
internal sealed class ConfigurationQueryDbContext(DbContextOptions<ConfigurationQueryDbContext> options, ModuleDbContextServices services)
    : ModuleQueryDbContext(options, services), IConfigurationQueryDb
{
    /// <inheritdoc />
    public override string Module => ConfigurationModuleInfo.Name;

    /// <inheritdoc />
    public IQueryable<DictionaryType> DictionaryTypes => Set<DictionaryType>();

    /// <inheritdoc />
    public IQueryable<SystemSettingValue> SettingValues => Set<SystemSettingValue>();

    /// <inheritdoc />
    public IQueryable<SystemSettingChange> SettingChanges => Set<SystemSettingChange>();

    /// <inheritdoc />
    public IQueryable<PreferenceValue> PreferenceValues => Set<PreferenceValue>();

    /// <inheritdoc />
    protected override void ConfigureModuleConventions(ModelConfigurationBuilder configurationBuilder) =>
        configurationBuilder.RegisterAllInConfigurationIdConverters();

    /// <inheritdoc />
    protected override void ConfigureModel(ModelBuilder modelBuilder) => ConfigurationModel.Configure(modelBuilder);
}

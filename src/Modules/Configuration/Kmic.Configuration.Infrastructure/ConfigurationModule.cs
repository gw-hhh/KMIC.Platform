using System.Reflection;
using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Application.Startup;
using Kmic.BuildingBlocks.Infrastructure.Hosting;
using Kmic.BuildingBlocks.Infrastructure.Modules;
using Kmic.BuildingBlocks.Infrastructure.Persistence;
using Kmic.Configuration.Application;
using Kmic.Configuration.Application.ContractQueries;
using Kmic.Configuration.Application.Dictionaries;
using Kmic.Configuration.Application.Preferences;
using Kmic.Configuration.Application.Settings;
using Kmic.Configuration.Contracts;
using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Configuration.Domain.Settings;
using Kmic.Configuration.Infrastructure.Dictionaries;
using Kmic.Configuration.Infrastructure.Persistence;
using Kmic.Configuration.Infrastructure.Persistence.Repositories;
using Kmic.Configuration.Infrastructure.Preferences;
using Kmic.Configuration.Infrastructure.Settings;
using Kmic.Identity.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Kmic.Configuration.Infrastructure;

/// <summary>
/// Configuration 模块的服务端注册类
/// </summary>
public sealed class ConfigurationModule : IModule
{
    /// <inheritdoc />
    public string Name => ConfigurationModuleInfo.Name;

    /// <inheritdoc />
    public Assembly ApplicationAssembly => typeof(IConfigurationQueryDb).Assembly;

    /// <inheritdoc />
    public void AddServices(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddModuleDbContexts<ConfigurationDbContext, ConfigurationQueryDbContext, IConfigurationQueryDb>();
        services.AddScoped<IDictionaryTypeRepository, DictionaryTypeRepository>();
        services.AddScoped<IDictionarySnapshots, DictionarySnapshotReader>();
        services.AddScoped<ISystemSettingRepository, SystemSettingRepository>();
        services.AddScoped<ISettings, SettingsReader>();
        services.AddScoped<IPreferenceRepository, PreferenceRepository>();
        services.AddScoped<IPreferenceLayers, PreferenceLayerReader>();
        services.AddScoped<IPreferences, PreferencesReader>();

        // Contracts 的实现：薄适配器，经 IContractQuerySender 调用本模块的契约查询
        services.AddScoped<IDictionaries, DictionaryLookup>();
        services.AddScoped<IStartupTask, SyncDictionaryTypesTask>();
        services.AddKmicOptions<DictionaryOptions>(DictionaryOptions.Section);
        services.AddKmicOptions<SettingOptions>(SettingOptions.Section);
        services.AddKmicOptions<PreferenceOptions>(PreferenceOptions.Section);
    }

    /// <inheritdoc />
    public void DeclareDependencies(IModuleDependencies dependencies)
    {
        ArgumentNullException.ThrowIfNull(dependencies);

        dependencies.DependsOn(IdentityModuleInfo.Name);
    }
}

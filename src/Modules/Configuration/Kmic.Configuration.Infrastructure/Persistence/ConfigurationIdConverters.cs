using Kmic.Configuration.Domain.Dictionaries;
using Kmic.Configuration.Domain.Preferences;
using Kmic.Configuration.Domain.Settings;
using Vogen;

namespace Kmic.Configuration.Infrastructure.Persistence;

[EfCoreConverter<DictionaryTypeId>]
[EfCoreConverter<DictionaryItemId>]
[EfCoreConverter<SystemSettingValueId>]
[EfCoreConverter<SettingChangeId>]
[EfCoreConverter<PreferenceValueId>]
internal sealed partial class ConfigurationIdConverters;

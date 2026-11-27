using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

public sealed class PreferenceCatalog
{
    private static readonly MethodInfo _bind = typeof(PreferenceCatalog).GetMethod(nameof(Bind), BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly FrozenDictionary<string, PreferenceDefinition> _byKey;
    private readonly FrozenDictionary<Type, PreferenceDefinition> _byType;

    private PreferenceCatalog(List<PreferenceDefinition> preferences)
    {
        _byKey = preferences.ToFrozenDictionary(p => p.Key, StringComparer.Ordinal);
        _byType = preferences.ToFrozenDictionary(p => p.ValueType);
    }

    public IReadOnlyCollection<PreferenceDefinition> All => _byKey.Values;

    public static PreferenceCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => t.IsDefined(typeof(PreferenceAttribute), inherit: false)));
    }

    public static PreferenceCatalog Build(IEnumerable<Type> preferenceTypes)
    {
        ArgumentNullException.ThrowIfNull(preferenceTypes);

        var problems = new List<string>();
        var preferences = preferenceTypes.Distinct().Select(t => Read(t, problems)).OfType<PreferenceDefinition>().ToList();
        foreach (var duplicated in preferences.GroupBy(p => p.Key, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"偏好 {duplicated.Key} 重复声明。");
        }

        return problems.Count > 0
            ? throw new DeclarationException("偏好声明", problems)
            : new PreferenceCatalog(preferences);
    }

    /// <summary>按值类型获取偏好定义，未注册时抛出异常。</summary>
    public PreferenceDefinition Get<T>()
        where T : class, IPreference<T> =>
        _byType.GetValueOrDefault(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} 不在偏好目录中，请检查其是否已标注 [Preference] 且所属模块已注册。");

    /// <summary>按键查找偏好定义，已移除的偏好返回 null。</summary>
    public PreferenceDefinition? Find(string key) => _byKey.GetValueOrDefault(key);

    private static PreferenceDefinition? Read(Type type, List<string> problems)
    {
        var declaration = type.GetCustomAttribute<PreferenceAttribute>()
            ?? throw new ArgumentException($"{type.Name} 未标注 [Preference]。", nameof(type));
        var countBefore = problems.Count;
        DeclaredValueRules.CheckKey(type, declaration.Key, problems);
        DeclaredValueRules.CheckShape(type, problems);
        if (declaration.Layers == PreferenceLayers.None || (declaration.Layers & ~PreferenceLayers.All) != PreferenceLayers.None)
        {
            problems.Add($"{type.Name}：Layers 必须是 System、Role、UserRole 的组合。");
        }

        if (!DeclaredValueRules.ImplementsSelf(type, typeof(IPreference<>)))
        {
            problems.Add($"{type.Name}：必须实现 IPreference<{type.Name}>。");
            return null;
        }

        var (defaultValue, validate) = ((object, Func<object, Result>))_bind.MakeGenericMethod(type).Invoke(null, null)!;
        DeclaredValueRules.CheckDefault(type, defaultValue, validate, problems);
        return problems.Count > countBefore ? null : new PreferenceDefinition(declaration, type, defaultValue, validate);
    }

    // 静态抽象成员须在泛型上下文中调用，此处封装为无类型参数委托
    private static (object Default, Func<object, Result> Validate) Bind<T>()
        where T : class, IPreference<T> =>
        (T.Default, value => T.Validate((T)value));
}

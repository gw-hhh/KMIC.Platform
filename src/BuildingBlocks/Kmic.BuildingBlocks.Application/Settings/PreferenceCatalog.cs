using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

/// <summary>
/// 偏好目录
/// </summary>
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

    /// <summary>
    /// 全部偏好
    /// </summary>
    public IReadOnlyCollection<PreferenceDefinition> All => _byKey.Values;

    /// <summary>
    /// 从程序集中读取
    /// </summary>
    public static PreferenceCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => t.IsDefined(typeof(PreferenceAttribute), inherit: false)));
    }

    /// <summary>
    /// 从给定类型中读取
    /// </summary>
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

    /// <summary>
    /// 按值类型取定义
    /// </summary>
    public PreferenceDefinition Get<T>()
        where T : class, IPreference<T> =>
        _byType.GetValueOrDefault(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} 不在偏好目录中，检查它是否标了 [Preference]、所在模块是否已注册。");

    /// <summary>
    /// 按键取定义，代码里已删除的偏好返回空
    /// </summary>
    public PreferenceDefinition? Find(string key) => _byKey.GetValueOrDefault(key);

    private static PreferenceDefinition? Read(Type type, List<string> problems)
    {
        var declaration = type.GetCustomAttribute<PreferenceAttribute>()
            ?? throw new ArgumentException($"{type.Name} 没有标 [Preference]。", nameof(type));
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

    // 静态抽象成员只能在泛型上下文里调用，这里包成不需要类型参数的委托
    private static (object Default, Func<object, Result> Validate) Bind<T>()
        where T : class, IPreference<T> =>
        (T.Default, value => T.Validate((T)value));
}

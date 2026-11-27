using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.BuildingBlocks.Application.Settings;

public sealed class SystemSettingCatalog
{
    private static readonly MethodInfo _bind = typeof(SystemSettingCatalog).GetMethod(nameof(Bind), BindingFlags.NonPublic | BindingFlags.Static)!;

    private readonly FrozenDictionary<string, SystemSettingDefinition> _byKey;
    private readonly FrozenDictionary<Type, SystemSettingDefinition> _byType;

    private SystemSettingCatalog(List<SystemSettingDefinition> settings)
    {
        _byKey = settings.ToFrozenDictionary(s => s.Key, StringComparer.Ordinal);
        _byType = settings.ToFrozenDictionary(s => s.ValueType);
    }

    public IReadOnlyCollection<SystemSettingDefinition> All => _byKey.Values;

    /// <summary>修改设置所需的权限点引用，供权限点目录校验。</summary>
    public IEnumerable<PermissionReference> PermissionReferences =>
        All.Select(s => new PermissionReference(s.EditPermission, $"系统设置 {s.Key}"));

    public static SystemSettingCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => t.IsDefined(typeof(SystemSettingAttribute), inherit: false)));
    }

    public static SystemSettingCatalog Build(IEnumerable<Type> settingTypes)
    {
        ArgumentNullException.ThrowIfNull(settingTypes);

        var problems = new List<string>();
        var settings = settingTypes.Distinct().Select(t => Read(t, problems)).OfType<SystemSettingDefinition>().ToList();
        foreach (var duplicated in settings.GroupBy(s => s.Key, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"设置 {duplicated.Key} 重复声明。");
        }

        return problems.Count > 0
            ? throw new DeclarationException("系统设置声明", problems)
            : new SystemSettingCatalog(settings);
    }

    /// <summary>按值类型获取设置定义，未注册时抛出异常。</summary>
    public SystemSettingDefinition Get<T>()
        where T : class, ISystemSetting<T> =>
        _byType.GetValueOrDefault(typeof(T))
            ?? throw new InvalidOperationException($"{typeof(T).Name} 不在设置目录中，请检查其是否已标注 [SystemSetting] 且所属模块已注册。");

    /// <summary>按键查找设置定义，已移除的设置返回 null。</summary>
    public SystemSettingDefinition? Find(string key) => _byKey.GetValueOrDefault(key);

    private static SystemSettingDefinition? Read(Type type, List<string> problems)
    {
        var declaration = type.GetCustomAttribute<SystemSettingAttribute>()
            ?? throw new ArgumentException($"{type.Name} 未标注 [SystemSetting]。", nameof(type));
        var countBefore = problems.Count;
        DeclaredValueRules.CheckKey(type, declaration.Key, problems);
        DeclaredValueRules.CheckShape(type, problems);
        if (string.IsNullOrWhiteSpace(declaration.EditPermission))
        {
            problems.Add($"{type.Name}：必须声明 EditPermission。");
        }

        if (!DeclaredValueRules.ImplementsSelf(type, typeof(ISystemSetting<>)))
        {
            problems.Add($"{type.Name}：必须实现 ISystemSetting<{type.Name}>。");
            return null;
        }

        var (defaultValue, validate) = ((object, Func<object, Result>))_bind.MakeGenericMethod(type).Invoke(null, null)!;
        DeclaredValueRules.CheckDefault(type, defaultValue, validate, problems);
        return problems.Count > countBefore ? null : new SystemSettingDefinition(declaration, type, defaultValue, validate);
    }

    // 静态抽象成员须在泛型上下文中调用，此处封装为无类型参数委托
    private static (object Default, Func<object, Result> Validate) Bind<T>()
        where T : class, ISystemSetting<T> =>
        (T.Default, value => T.Validate((T)value));
}

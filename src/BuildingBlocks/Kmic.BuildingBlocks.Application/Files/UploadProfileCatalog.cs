using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.BuildingBlocks.Application.Files;

public sealed class UploadProfileCatalog
{
    private readonly FrozenDictionary<string, UploadProfile> _byName;

    private UploadProfileCatalog(IReadOnlyList<UploadProfile> profiles)
    {
        All = profiles;
        _byName = profiles.ToFrozenDictionary(p => p.Name, StringComparer.Ordinal);
    }

    public static UploadProfileCatalog Empty { get; } = new([]);

    /// <summary>全部上传配置，按名称排序。</summary>
    public IReadOnlyList<UploadProfile> All { get; }

    /// <summary>所有上传配置中的最大单文件上限。</summary>
    public long LargestMaxBytes => All.Count == 0 ? 0 : All.Max(p => p.MaxBytes);

    /// <summary>上传配置引用的权限点，供权限点目录校验。</summary>
    public IEnumerable<PermissionReference> PermissionReferences => All.Select(p => new PermissionReference(p.Permission, $"上传配置 {p.Name}"));

    public UploadProfile? Find(string name) => _byName.GetValueOrDefault(name);

    public static UploadProfileCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);

        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()));
    }

    internal static UploadProfileCatalog Build(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        var problems = new List<string>();
        var profiles = types
            .Distinct()
            .SelectMany(t => t.GetCustomAttributes<UploadProfileAttribute>(inherit: false).Select(a => Read(t, a, problems)))
            .OfType<UploadProfile>()
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .ToList();
        foreach (var duplicate in profiles.GroupBy(p => p.Name, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"上传配置名“{duplicate.Key}”声明了 {duplicate.Count()} 次。");
        }

        return problems.Count > 0 ? throw new DeclarationException("上传配置声明", problems) : new UploadProfileCatalog(profiles);
    }

    private static UploadProfile? Read(Type owner, UploadProfileAttribute attribute, List<string> problems)
    {
        var module = ModuleNames.Of(owner);
        var name = attribute.Name;
        var before = problems.Count;
        if (!name.StartsWith(module + ".", StringComparison.Ordinal) || name.Length == module.Length + 1)
        {
            problems.Add($"上传配置名“{name}”须以“{module}.”开头。");
        }

        if (string.IsNullOrWhiteSpace(attribute.DisplayName))
        {
            problems.Add($"上传配置 {name} 必须设置显示名。");
        }

        if (attribute.MaxBytes is < 1 or > UploadProfileAttribute.AbsoluteMaxBytes)
        {
            problems.Add($"上传配置 {name} 的 MaxBytes 须在 1 到 {UploadProfileAttribute.AbsoluteMaxBytes} 之间。");
        }

        if (attribute.AllowedTypes.Length == 0)
        {
            problems.Add($"上传配置 {name} 必须设置 AllowedTypes，至少允许一种文件类型。");
        }

        foreach (var code in attribute.AllowedTypes.Where(c => FileTypes.Find(c) is null))
        {
            problems.Add($"上传配置 {name} 允许的文件类型“{code}”不受支持，只能使用 FileType 中的编码。");
        }

        if (attribute.AllowedTypes.Distinct(StringComparer.Ordinal).Count() != attribute.AllowedTypes.Length)
        {
            problems.Add($"上传配置 {name} 的 AllowedTypes 存在重复项。");
        }

        if (string.IsNullOrWhiteSpace(attribute.Permission))
        {
            problems.Add($"上传配置 {name} 必须设置 Permission，应指定编辑对应业务对象的权限点。");
        }

        return problems.Count > before
            ? null
            : new UploadProfile(name, attribute.DisplayName, module, attribute.MaxBytes, [.. attribute.AllowedTypes], attribute.Permission);
    }
}

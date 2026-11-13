using System.Collections.Frozen;
using System.Reflection;
using Kmic.BuildingBlocks.Application.Authorization;

namespace Kmic.BuildingBlocks.Application.Files;

/// <summary>
/// 上传档案目录
/// </summary>
public sealed class UploadProfileCatalog
{
    private readonly FrozenDictionary<string, UploadProfile> _byName;

    private UploadProfileCatalog(IReadOnlyList<UploadProfile> profiles)
    {
        All = profiles;
        _byName = profiles.ToFrozenDictionary(p => p.Name, StringComparer.Ordinal);
    }

    /// <summary>
    /// 没有任何档案的目录
    /// </summary>
    public static UploadProfileCatalog Empty { get; } = new([]);

    /// <summary>
    /// 全部档案，按名称排好
    /// </summary>
    public IReadOnlyList<UploadProfile> All { get; }

    /// <summary>
    /// 全部档案里最大的单文件上限：上传端点的请求体上限用它，真正的限制在读流时按各自档案的上限截断
    /// </summary>
    public long LargestMaxBytes => All.Count == 0 ? 0 : All.Max(p => p.MaxBytes);

    /// <summary>
    /// 档案声明里引用的权限点，交给权限点目录核对
    /// </summary>
    public IEnumerable<PermissionReference> PermissionReferences => All.Select(p => new PermissionReference(p.Permission, $"上传档案 {p.Name}"));

    /// <summary>
    /// 按名称取，不存在时返回空
    /// </summary>
    /// <param name="name">档案名</param>
    public UploadProfile? Find(string name) => _byName.GetValueOrDefault(name);

    /// <summary>
    /// 从各模块的 Application 程序集中读取档案声明
    /// </summary>
    /// <param name="assemblies">各模块的 Application 程序集</param>
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
            problems.Add($"上传档案名“{duplicate.Key}”声明了 {duplicate.Count()} 次。");
        }

        return problems.Count > 0 ? throw new DeclarationException("上传档案声明", problems) : new UploadProfileCatalog(profiles);
    }

    private static UploadProfile? Read(Type owner, UploadProfileAttribute attribute, List<string> problems)
    {
        var module = ModuleNames.Of(owner);
        var name = attribute.Name;
        var before = problems.Count;
        if (!name.StartsWith(module + ".", StringComparison.Ordinal) || name.Length == module.Length + 1)
        {
            problems.Add($"上传档案名“{name}”要以“{module}.”开头。");
        }

        if (string.IsNullOrWhiteSpace(attribute.DisplayName))
        {
            problems.Add($"上传档案 {name} 要写显示名。");
        }

        if (attribute.MaxBytes is < 1 or > UploadProfileAttribute.AbsoluteMaxBytes)
        {
            problems.Add($"上传档案 {name} 的 MaxBytes 要在 1 到 {UploadProfileAttribute.AbsoluteMaxBytes} 之间。");
        }

        if (attribute.AllowedTypes.Length == 0)
        {
            problems.Add($"上传档案 {name} 要写 AllowedTypes，至少允许一种文件类型。");
        }

        foreach (var code in attribute.AllowedTypes.Where(c => FileTypes.Find(c) is null))
        {
            problems.Add($"上传档案 {name} 允许的文件类型“{code}”框架不认识，只能用 FileType 里的编码。");
        }

        if (attribute.AllowedTypes.Distinct(StringComparer.Ordinal).Count() != attribute.AllowedTypes.Length)
        {
            problems.Add($"上传档案 {name} 的 AllowedTypes 有重复。");
        }

        if (string.IsNullOrWhiteSpace(attribute.Permission))
        {
            problems.Add($"上传档案 {name} 要写 Permission：上传要有编辑这类业务对象的权限点。");
        }

        return problems.Count > before
            ? null
            : new UploadProfile(name, attribute.DisplayName, module, attribute.MaxBytes, [.. attribute.AllowedTypes], attribute.Permission);
    }
}

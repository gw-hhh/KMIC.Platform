using System.Collections.Frozen;
using System.Reflection;
using System.Text.RegularExpressions;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Application.Authorization;

/// <summary>
/// 全部模块的权限点目录：启动时扫描并校验，同步到 Identity、导出给前端都以它为准
/// </summary>
public sealed partial class PermissionCatalog
{
    private readonly FrozenDictionary<string, PermissionDefinition> _byCode;

    private PermissionCatalog(IEnumerable<PermissionDefinition> permissions) =>
        _byCode = permissions.ToFrozenDictionary(p => p.Code, StringComparer.Ordinal);

    /// <summary>
    /// 全部权限点
    /// </summary>
    public IReadOnlyCollection<PermissionDefinition> All => _byCode.Values;

    /// <summary>
    /// 扫描程序集中标了 [PermissionGroup] 的常量类
    /// </summary>
    public static PermissionCatalog FromAssemblies(IEnumerable<Assembly> assemblies)
    {
        ArgumentNullException.ThrowIfNull(assemblies);
        return Build(assemblies.Distinct().SelectMany(a => a.GetTypes()).Where(t => t.IsDefined(typeof(PermissionGroupAttribute), inherit: false)));
    }

    /// <summary>
    /// 读取并校验格式、归属与唯一性
    /// </summary>
    public static PermissionCatalog Build(IEnumerable<Type> groupTypes)
    {
        ArgumentNullException.ThrowIfNull(groupTypes);

        var problems = new List<string>();
        var permissions = new List<PermissionDefinition>();
        foreach (var type in groupTypes.Distinct())
        {
            permissions.AddRange(ReadGroup(type, problems));
        }

        foreach (var duplicated in permissions.GroupBy(p => p.Code, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            problems.Add($"权限点 {duplicated.Key} 重复声明。");
        }

        foreach (var duplicated in permissions.GroupBy(p => (p.Module, p.Group, p.DisplayName)).Where(g => g.Count() > 1))
        {
            problems.Add($"分组“{duplicated.Key.Group}”中显示名“{duplicated.Key.DisplayName}”重复。");
        }

        if (problems.Count > 0)
        {
            throw new DeclarationException("权限点声明", problems);
        }

        return new PermissionCatalog(permissions);
    }

    /// <summary>
    /// 权限点是否存在
    /// </summary>
    public bool Contains(string code) => _byCode.ContainsKey(code);

    /// <summary>
    /// 与用例目录、资源动作核对：引用的权限点都存在、没有无人使用的权限点、敏感权限点的用例都审计
    /// </summary>
    public void Verify(UseCaseCatalog useCases, IEnumerable<string> resourceActionPermissions)
    {
        ArgumentNullException.ThrowIfNull(useCases);
        ArgumentNullException.ThrowIfNull(resourceActionPermissions);

        var problems = new List<string>();
        var used = new HashSet<string>(StringComparer.Ordinal);
        foreach (var useCase in useCases.All)
        {
            if (useCase.Permission is not { } code)
            {
                continue;
            }

            used.Add(code);
            if (!_byCode.TryGetValue(code, out var permission))
            {
                problems.Add($"{useCase.Name}：[RequirePermission] 引用的权限点 {code} 不存在。");
            }
            else if (permission.Sensitive && !IsAudited(useCase))
            {
                // 敏感操作必须留痕（05-09 D1、D5）
                var required = useCase.Kind == MessageKind.Command ? "[Audited]" : "[AuditedRead]";
                problems.Add($"{useCase.Name}：需要敏感权限点 {code}，必须 {required}。");
            }
        }

        foreach (var code in resourceActionPermissions.Distinct(StringComparer.Ordinal))
        {
            used.Add(code);
            if (!_byCode.ContainsKey(code))
            {
                problems.Add($"资源动作引用的权限点 {code} 不存在。");
            }
        }

        problems.AddRange(_byCode.Keys.Where(code => !used.Contains(code)).Order(StringComparer.Ordinal)
            .Select(code => $"权限点 {code} 没有被任何用例或资源动作使用。"));

        if (problems.Count > 0)
        {
            throw new DeclarationException("权限点声明", problems);
        }
    }

    private static bool IsAudited(UseCase useCase) =>
        useCase.Kind == MessageKind.Command ? useCase.Audited == true : useCase.AuditedRead;

    private static IEnumerable<PermissionDefinition> ReadGroup(Type type, List<string> problems)
    {
        var group = type.GetCustomAttribute<PermissionGroupAttribute>()!;
        foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly))
        {
            var attribute = field.GetCustomAttribute<PermissionAttribute>();
            if (!field.IsLiteral || field.FieldType != typeof(string) || attribute is null)
            {
                problems.Add($"{type.Name}.{field.Name}：权限点必须是标了 [Permission] 的 string 常量。");
                continue;
            }

            var code = (string)field.GetRawConstantValue()!;
            if (!CodePattern().IsMatch(code))
            {
                problems.Add($"{type.Name}.{field.Name}：权限点 {code} 不符合“模块.资源.动作”格式（三段，每段大驼峰）。");
                continue;
            }

            if (!code.StartsWith(group.Module + ".", StringComparison.Ordinal))
            {
                problems.Add($"{type.Name}.{field.Name}：权限点 {code} 的第一段必须是本模块名 {group.Module}。");
                continue;
            }

            yield return new PermissionDefinition(code, group.Module, group.DisplayName, attribute.DisplayName, attribute.Sensitive);
        }
    }

    [GeneratedRegex(@"^[A-Z][A-Za-z0-9]*(\.[A-Z][A-Za-z0-9]*){2}$", RegexOptions.CultureInvariant | RegexOptions.ExplicitCapture, matchTimeoutMilliseconds: 100)]
    private static partial Regex CodePattern();
}

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.BuildingBlocks.Application.UseCases;

/// <summary>
/// 引用解析查询的返回类型规则
/// </summary>
/// <remarks>
/// Reference 查询不检查权限点、不套数据范围，所以只能返回 ReferenceInfo（ID、显示名、编码、状态）或它的集合、字典，
/// 不然它会变成读取别的模块数据的通道。确有例外的在特性上用 ResultExemption 写明原因
/// </remarks>
internal static class ReferenceResultRule
{
    private static readonly HashSet<Type> _containers =
    [
        typeof(IEnumerable<>),
        typeof(ICollection<>),
        typeof(IList<>),
        typeof(IReadOnlyCollection<>),
        typeof(IReadOnlyList<>),
        typeof(List<>),
        typeof(IDictionary<,>),
        typeof(IReadOnlyDictionary<,>),
        typeof(Dictionary<,>),
    ];

    public static void Check(Type queryType, string name, ContractQueryAttribute attribute, List<string> problems)
    {
        var exemption = attribute.ResultExemption;
        if (attribute.Access != ContractAccess.Reference)
        {
            if (exemption is not null)
            {
                problems.Add($"{name}：ResultExemption 只能用在 [ContractQuery(ContractAccess.Reference)] 上。");
            }

            return;
        }

        if (exemption is not null && string.IsNullOrWhiteSpace(exemption))
        {
            problems.Add($"{name}：ResultExemption 必须写明原因。");
            return;
        }

        var response = queryType.GetInterfaces()
            .First(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IQuery<>))
            .GetGenericArguments()[0];
        var standard = IsReferenceShape(response);
        if (!standard && exemption is null)
        {
            problems.Add(
                $"{name}：引用解析查询的返回类型只能是 ReferenceInfo 或它的集合、字典，现在是 {response.Name}。确有例外的，在 [ContractQuery] 上用 ResultExemption 写明原因。");
        }

        if (standard && exemption is not null)
        {
            problems.Add($"{name}：返回类型已经是 ReferenceInfo，不需要 ResultExemption，去掉它。");
        }
    }

    private static bool IsReferenceShape(Type type)
    {
        if (type == typeof(ReferenceInfo))
        {
            return true;
        }

        if (type.IsArray)
        {
            return type.GetElementType() == typeof(ReferenceInfo);
        }

        return type.IsGenericType && _containers.Contains(type.GetGenericTypeDefinition()) && type.GetGenericArguments()[^1] == typeof(ReferenceInfo);
    }
}

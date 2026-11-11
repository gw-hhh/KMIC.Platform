using System.Reflection;

namespace Kmic.BuildingBlocks.Application.Integration;

/// <summary>
/// 一种集成消息的声明
/// </summary>
/// <param name="Name">消息名称，如 Customer.CustomerChanged</param>
/// <param name="Version">版本号</param>
/// <param name="IsRequest">是否请求型消息</param>
/// <param name="ClrType">消息类型</param>
/// <param name="Module">定义它的模块</param>
/// <param name="Subjects">存涉及对象 ID 的属性，没有标 [MessageSubjects] 时为空</param>
/// <param name="OrderingKey">作为顺序键的属性，没有标 [OrderingKey] 时为空</param>
internal sealed record IntegrationMessageType(
    string Name,
    int Version,
    bool IsRequest,
    Type ClrType,
    string Module,
    PropertyInfo? Subjects,
    PropertyInfo? OrderingKey = null)
{
    /// <summary>
    /// 消息涉及的对象 ID
    /// </summary>
    public IReadOnlyList<Guid>? SubjectIdsOf(object message) =>
        Subjects?.GetValue(message) switch
        {
            null => null,
            Guid id => [id],
            IEnumerable<Guid> ids => [.. ids],
            _ => throw new InvalidOperationException($"{ClrType.Name}.{Subjects.Name} 不是 Guid 或 Guid 的集合。"),
        };

    /// <summary>
    /// 消息声明的顺序键
    /// </summary>
    public string? OrderingKeyOf(object message) => OrderingKey?.GetValue(message)?.ToString();
}

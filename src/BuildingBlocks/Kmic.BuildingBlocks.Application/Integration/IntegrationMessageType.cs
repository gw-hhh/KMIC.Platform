using System.Reflection;

namespace Kmic.BuildingBlocks.Application.Integration;

internal sealed record IntegrationMessageType(
    string Name,
    int Version,
    bool IsRequest,
    Type ClrType,
    string Module,
    PropertyInfo? Subjects,
    PropertyInfo? OrderingKey = null,
    int? SecretLifetimeSeconds = null)
{
    // 载荷含一次性短时凭据，日志与管理视图禁止输出载荷内容
    public bool CarriesSecret => SecretLifetimeSeconds is not null;

    public IReadOnlyList<Guid>? SubjectIdsOf(object message) =>
        Subjects?.GetValue(message) switch
        {
            null => null,
            Guid id => [id],
            IEnumerable<Guid> ids => [.. ids],
            _ => throw new InvalidOperationException($"{ClrType.Name}.{Subjects.Name} 不是 Guid 或 Guid 的集合。"),
        };

    public string? OrderingKeyOf(object message) => OrderingKey?.GetValue(message)?.ToString();
}

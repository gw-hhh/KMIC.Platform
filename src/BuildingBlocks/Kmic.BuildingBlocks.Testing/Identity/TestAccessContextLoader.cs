using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>
/// 测试用的访问上下文加载器
/// </summary>
/// <remarks>
/// 先登记身份，作业执行时按用户和角色取回；没登记或已移除的当作用户已禁用
/// </remarks>
public sealed class TestAccessContextLoader : IAccessContextLoader
{
    private readonly ConcurrentDictionary<(UserId, RoleId), IAccessContext> _identities = new();

    /// <summary>
    /// 登记或替换一个身份
    /// </summary>
    public void Register(TestIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identities[(identity.UserId, identity.RoleId)] = identity;
    }

    /// <summary>
    /// 移除身份，模拟用户被禁用或角色被收回
    /// </summary>
    public void Remove(TestIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identities.TryRemove((identity.UserId, identity.RoleId), out _);
    }

    /// <inheritdoc />
    public Task<IAccessContext?> LoadForJobAsync(UserId user, RoleId role, CancellationToken ct) =>
        Task.FromResult(_identities.GetValueOrDefault((user, role)));
}

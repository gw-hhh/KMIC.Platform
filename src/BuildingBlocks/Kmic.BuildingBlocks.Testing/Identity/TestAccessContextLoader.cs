using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>
/// 测试用的访问上下文加载器
/// </summary>
/// <remarks>
/// 先登记身份，请求和作业执行时按用户、会话和角色取回；没登记或已移除的当作用户已禁用或会话失效。
/// 登记过但角色被标记为已收回的，请求得到“当前角色不可用”，作业不执行
/// </remarks>
public sealed class TestAccessContextLoader : IAccessContextLoader
{
    private readonly ConcurrentDictionary<(UserId, RoleId), TestIdentity> _identities = new();
    private readonly ConcurrentDictionary<(UserId, RoleId), byte> _revoked = new();

    /// <summary>
    /// 登记或替换一个身份
    /// </summary>
    public void Register(TestIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identities[(identity.UserId, identity.RoleId)] = identity;
        _revoked.TryRemove((identity.UserId, identity.RoleId), out _);
    }

    /// <summary>
    /// 移除身份，模拟用户被禁用或会话失效
    /// </summary>
    public void Remove(TestIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identities.TryRemove((identity.UserId, identity.RoleId), out _);
    }

    /// <summary>
    /// 收回身份的当前角色：用户还在，会话还有效，但这个角色不能再用
    /// </summary>
    public void RevokeRole(TestIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _revoked[(identity.UserId, identity.RoleId)] = 0;
    }

    /// <inheritdoc />
    public Task<AccessLoadResult> LoadForRequestAsync(UserId user, SessionId session, RoleId role, CancellationToken ct)
    {
        if (!_identities.TryGetValue((user, role), out var identity) || identity.SessionId != session)
        {
            return Task.FromResult(AccessLoadResult.SessionInvalid());
        }

        return Task.FromResult(_revoked.ContainsKey((user, role))
            ? AccessLoadResult.CurrentRoleUnavailable(identity.WithoutRolePrivileges())
            : AccessLoadResult.Loaded(identity));
    }

    /// <inheritdoc />
    public Task<IAccessContext?> LoadForJobAsync(UserId user, RoleId role, CancellationToken ct) =>
        Task.FromResult(_revoked.ContainsKey((user, role)) ? null : _identities.GetValueOrDefault((user, role)) as IAccessContext);
}

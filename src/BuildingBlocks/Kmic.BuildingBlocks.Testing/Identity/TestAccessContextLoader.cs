using System.Collections.Concurrent;
using Kmic.BuildingBlocks.Application.Access;
using Kmic.BuildingBlocks.Domain.Identifiers;

namespace Kmic.BuildingBlocks.Testing.Identity;

/// <summary>测试用访问上下文加载器（未登记或已移除的身份均视为用户已禁用或会话失效）。</summary>
public sealed class TestAccessContextLoader : IAccessContextLoader
{
    private readonly ConcurrentDictionary<(UserId, RoleId), TestIdentity> _identities = new();
    private readonly ConcurrentDictionary<(UserId, RoleId), byte> _revoked = new();

    /// <summary>注册或更新测试身份。</summary>
    public void Register(TestIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identities[(identity.UserId, identity.RoleId)] = identity;
        _revoked.TryRemove((identity.UserId, identity.RoleId), out _);
    }

    /// <summary>移除指定身份，用于模拟用户禁用或会话失效。</summary>
    public void Remove(TestIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _identities.TryRemove((identity.UserId, identity.RoleId), out _);
    }

    /// <summary>吊销身份的当前角色：保留用户与会话有效性，但该角色不再可用。</summary>
    public void RevokeRole(TestIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        _revoked[(identity.UserId, identity.RoleId)] = 0;
    }

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

    public Task<IAccessContext?> LoadForJobAsync(UserId user, RoleId role, CancellationToken ct) =>
        Task.FromResult(_revoked.ContainsKey((user, role)) ? null : _identities.GetValueOrDefault((user, role)) as IAccessContext);
}

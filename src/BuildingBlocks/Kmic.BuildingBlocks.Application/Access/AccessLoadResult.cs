namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>访问上下文的加载结果。</summary>
public sealed record AccessLoadResult
{
    private static readonly AccessLoadResult _sessionInvalid = new() { Status = AccessLoadStatus.SessionInvalid };

    private AccessLoadResult()
    {
    }

    public required AccessLoadStatus Status { get; init; }

    /// <summary>访问上下文；会话无效时为 null。</summary>
    public IAccessContext? Access { get; init; }

    public static AccessLoadResult SessionInvalid() => _sessionInvalid;

    public static AccessLoadResult Loaded(IAccessContext access)
    {
        ArgumentNullException.ThrowIfNull(access);
        return new AccessLoadResult { Status = AccessLoadStatus.Loaded, Access = access };
    }

    /// <summary>构造“当前角色不可用”结果，上下文仅保留查看个人信息与切换角色所需权限。</summary>
    public static AccessLoadResult CurrentRoleUnavailable(IAccessContext access)
    {
        ArgumentNullException.ThrowIfNull(access);
        return new AccessLoadResult { Status = AccessLoadStatus.CurrentRoleUnavailable, Access = access };
    }
}

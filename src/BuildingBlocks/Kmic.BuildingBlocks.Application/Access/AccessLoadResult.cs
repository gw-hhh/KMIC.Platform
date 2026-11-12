namespace Kmic.BuildingBlocks.Application.Access;

/// <summary>
/// 请求的访问上下文加载结果
/// </summary>
public sealed record AccessLoadResult
{
    private static readonly AccessLoadResult _sessionInvalid = new() { Status = AccessLoadStatus.SessionInvalid };

    private AccessLoadResult()
    {
    }

    /// <summary>
    /// 结果
    /// </summary>
    public required AccessLoadStatus Status { get; init; }

    /// <summary>
    /// 访问上下文，会话无效时为空
    /// </summary>
    public IAccessContext? Access { get; init; }

    /// <summary>
    /// 会话无效
    /// </summary>
    public static AccessLoadResult SessionInvalid() => _sessionInvalid;

    /// <summary>
    /// 加载成功
    /// </summary>
    public static AccessLoadResult Loaded(IAccessContext access)
    {
        ArgumentNullException.ThrowIfNull(access);
        return new AccessLoadResult { Status = AccessLoadStatus.Loaded, Access = access };
    }

    /// <summary>
    /// 当前角色已不可用
    /// </summary>
    /// <remarks>
    /// 访问上下文是用户自己的、没有这个角色的权限和数据范围的版本：只够查看自己的信息和切换角色
    /// </remarks>
    public static AccessLoadResult CurrentRoleUnavailable(IAccessContext access)
    {
        ArgumentNullException.ThrowIfNull(access);
        return new AccessLoadResult { Status = AccessLoadStatus.CurrentRoleUnavailable, Access = access };
    }
}

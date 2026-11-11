namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 幂等记录
/// </summary>
/// <remarks>
/// 只用来建表，读写都是框架里的 SQL
/// </remarks>
internal sealed class IdempotencyRecord
{
    /// <summary>
    /// ID
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 用户
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 角色
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 消息名
    /// </summary>
    public string Operation { get; set; } = "";

    /// <summary>
    /// 幂等键
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// 请求摘要
    /// </summary>
    public byte[] RequestHash { get; set; } = [];

    /// <summary>
    /// 状态
    /// </summary>
    public string Status { get; set; } = "";

    /// <summary>
    /// 占用令牌
    /// </summary>
    public Guid LeaseToken { get; set; }

    /// <summary>
    /// 占用到期时间
    /// </summary>
    public DateTimeOffset LeaseExpiresAt { get; set; }

    /// <summary>
    /// 响应值
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// 创建时间
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 完成时间
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
}

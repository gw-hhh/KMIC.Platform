namespace Kmic.BuildingBlocks.Infrastructure.Persistence;

/// <summary>
/// 幂等记录：每个模块 schema 中一张，与业务数据同一事务标记完成；只用来建表，读写都是框架的原生 SQL
/// </summary>
internal sealed class IdempotencyRecord
{
    /// <summary>
    /// 主键
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// 发起请求的用户
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// 发起请求时的当前角色
    /// </summary>
    public Guid RoleId { get; set; }

    /// <summary>
    /// 消息全名，如 Sample.SubmitSampleCommand
    /// </summary>
    public string Operation { get; set; } = "";

    /// <summary>
    /// 幂等键
    /// </summary>
    public string Key { get; set; } = "";

    /// <summary>
    /// 请求摘要（SHA-256），同键不同请求时拒绝
    /// </summary>
    public byte[] RequestHash { get; set; } = [];

    /// <summary>
    /// Processing 或 Completed
    /// </summary>
    public string Status { get; set; } = "";

    /// <summary>
    /// 当前持有者的令牌
    /// </summary>
    public Guid LeaseToken { get; set; }

    /// <summary>
    /// 租约到期时刻，到期后其他请求可以接管
    /// </summary>
    public DateTimeOffset LeaseExpiresAt { get; set; }

    /// <summary>
    /// 成功时响应值的 JSON；没有值的命令为空
    /// </summary>
    public string? ResponseBody { get; set; }

    /// <summary>
    /// 创建时刻
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; }

    /// <summary>
    /// 完成时刻
    /// </summary>
    public DateTimeOffset? CompletedAt { get; set; }

    /// <summary>
    /// 过期时刻，之后同一个键按新请求处理，由清理任务删除
    /// </summary>
    public DateTimeOffset ExpiresAt { get; set; }
}

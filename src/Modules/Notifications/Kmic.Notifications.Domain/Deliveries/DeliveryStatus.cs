using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>
/// 投递的状态
/// </summary>
public sealed class DeliveryStatus : Enumeration<DeliveryStatus>
{
    /// <summary>
    /// 待发送：在队列里等投递工作器领取
    /// </summary>
    public static readonly DeliveryStatus Pending = new("Pending", "待发送");

    /// <summary>
    /// 发送中：已被领取，正在调用渠道
    /// </summary>
    public static readonly DeliveryStatus Sending = new("Sending", "发送中");

    /// <summary>
    /// 已发送
    /// </summary>
    public static readonly DeliveryStatus Sent = new("Sent", "已发送");

    /// <summary>
    /// 失败：永久错误或重试次数用完，等人工处理
    /// </summary>
    public static readonly DeliveryStatus Failed = new("Failed", "失败");

    /// <summary>
    /// 已跳过：收件人没有地址，或者用户已禁用
    /// </summary>
    public static readonly DeliveryStatus Skipped = new("Skipped", "已跳过");

    /// <summary>
    /// 已捕获：非生产环境不真的发送，内容留在捕获表里
    /// </summary>
    public static readonly DeliveryStatus Captured = new("Captured", "已捕获");

    private DeliveryStatus(string code, string name)
        : base(code, name)
    {
    }
}

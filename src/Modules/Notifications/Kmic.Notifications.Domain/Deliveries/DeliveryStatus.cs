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
    /// 已被工作器领取，尚未记录本次处理结果
    /// </summary>
    public static readonly DeliveryStatus Sending = new("Sending", "发送中");

    /// <summary>
    /// 已发送
    /// </summary>
    public static readonly DeliveryStatus Sent = new("Sent", "已发送");

    /// <summary>
    /// 已停止自动重试，等待人工处理
    /// </summary>
    public static readonly DeliveryStatus Failed = new("Failed", "失败");

    /// <summary>
    /// 已跳过：用户不存在、已禁用或缺少收件地址
    /// </summary>
    public static readonly DeliveryStatus Skipped = new("Skipped", "已跳过");

    /// <summary>
    /// 已捕获：内容已保存到捕获表，未实际发送
    /// </summary>
    public static readonly DeliveryStatus Captured = new("Captured", "已捕获");

    private DeliveryStatus(string code, string name)
        : base(code, name)
    {
    }
}

using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>
/// 投递被跳过的原因
/// </summary>
public sealed class DeliverySkipReason : Enumeration<DeliverySkipReason>
{
    /// <summary>
    /// 收件人在这个渠道没有地址：没有邮箱，或者没有手机号
    /// </summary>
    public static readonly DeliverySkipReason NoAddress = new("NoAddress", "没有地址");

    /// <summary>
    /// 收件人的账号已经禁用
    /// </summary>
    public static readonly DeliverySkipReason UserDisabled = new("UserDisabled", "用户已禁用");

    /// <summary>
    /// 收件人不是系统里的用户，也没有给地址
    /// </summary>
    public static readonly DeliverySkipReason UnknownUser = new("UnknownUser", "用户不存在");

    private DeliverySkipReason(string code, string name)
        : base(code, name)
    {
    }
}

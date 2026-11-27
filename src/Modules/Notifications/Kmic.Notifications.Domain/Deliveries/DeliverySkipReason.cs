using Kmic.BuildingBlocks.Domain.Enumerations;

namespace Kmic.Notifications.Domain.Deliveries;

/// <summary>投递被跳过的原因</summary>
public sealed class DeliverySkipReason : Enumeration<DeliverySkipReason>
{
    /// <summary>缺少该渠道所需的联系方式（邮箱或手机号）</summary>
    public static readonly DeliverySkipReason NoAddress = new("NoAddress", "无收件地址");

    public static readonly DeliverySkipReason UserDisabled = new("UserDisabled", "用户已禁用");

    public static readonly DeliverySkipReason UnknownUser = new("UnknownUser", "用户不存在");

    private DeliverySkipReason(string code, string name)
        : base(code, name)
    {
    }
}

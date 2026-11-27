using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.MessagingCapacity;

/// <summary>数据库已分配容量配置，供消息容量保护判断存储是否紧张（0 表示未配置）。</summary>
/// <param name="CapacityBytes">已分配容量，0 或不少于 1 GiB</param>
[SystemSetting("Operations.MessagingCapacity", "消息容量保护", Group = "运维", EditPermission = OperationsPermissions.MessagingCapacityManage)]
public sealed record MessagingCapacitySetting(
    [property: SettingField("数据库已分配容量", Unit = "字节")] long CapacityBytes = 0) : ISystemSetting<MessagingCapacitySetting>
{
    /// <summary>容量下限，防止单位填错导致立即触发熔断。</summary>
    public const long MinBytes = 1L << 30;

    public static MessagingCapacitySetting Default { get; } = new();

    public static Result Validate(MessagingCapacitySetting value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.CapacityBytes is 0 or >= MinBytes
            ? Result.Success()
            : CommonErrors.ValidationFailedWith([new FieldError("capacityBytes", "Range", "容量须设为 0（不校验），或不少于 1 GiB（1073741824 字节）")]);
    }
}

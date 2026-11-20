using Kmic.BuildingBlocks.Application.Settings;
using Kmic.BuildingBlocks.Domain.Errors;
using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Operations.Application.MessagingCapacity;

/// <summary>
/// 数据库的已分配容量，消息写入的容量保护据此判断空间是否紧张
/// </summary>
/// <remarks>
/// 部署执行机定期读取 RDS 已分配的存储容量，经 <c>Kmic.Operator</c> 写入；0 表示没有设置，容量保护不判断
/// </remarks>
/// <param name="CapacityBytes">已分配容量，0 或不少于 1 GiB</param>
[SystemSetting("Operations.MessagingCapacity", "消息容量保护", Group = "运维", EditPermission = OperationsPermissions.MessagingCapacityManage)]
public sealed record MessagingCapacitySetting(
    [property: SettingField("数据库已分配容量", Unit = "字节")] long CapacityBytes = 0) : ISystemSetting<MessagingCapacitySetting>
{
    /// <summary>
    /// 容量的下限：小于它多半是单位写错了，设下去会立刻触发熔断
    /// </summary>
    public const long MinBytes = 1L << 30;

    /// <inheritdoc />
    public static MessagingCapacitySetting Default { get; } = new();

    /// <inheritdoc />
    public static Result Validate(MessagingCapacitySetting value)
    {
        ArgumentNullException.ThrowIfNull(value);

        return value.CapacityBytes is 0 or >= MinBytes
            ? Result.Success()
            : CommonErrors.ValidationFailedWith([new FieldError("capacityBytes", "Range", "容量填 0（不判断），或不少于 1 GiB（1073741824 字节）")]);
    }
}

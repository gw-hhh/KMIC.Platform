namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>
/// 业务时区配置
/// </summary>
public sealed class BusinessTimeOptions
{
    /// <summary>
    /// 配置节名称，时区写在 Kmic:BusinessTimeZone
    /// </summary>
    public const string Section = "Kmic";

    /// <summary>
    /// 业务时区的 IANA 名称
    /// </summary>
    public string BusinessTimeZone { get; set; } = "Asia/Shanghai";
}

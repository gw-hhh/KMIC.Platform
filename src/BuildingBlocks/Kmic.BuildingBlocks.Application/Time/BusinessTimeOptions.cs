namespace Kmic.BuildingBlocks.Application.Time;

/// <summary>业务时区配置（配置项为 Kmic:BusinessTimeZone）。</summary>
public sealed class BusinessTimeOptions
{
    public const string Section = "Kmic";

    /// <summary>业务时区 IANA 标识。</summary>
    public string BusinessTimeZone { get; set; } = "Asia/Shanghai";
}

namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>消息写入容量保护状态，供指标量表读取。</summary>
/// <param name="State">状态码：0 正常、1 软线、2 硬线。</param>
/// <param name="SampleAge">判断结果距上次更新的时长。</param>
/// <param name="CapacityAge">容量配置距上次更新的时长；未配置容量时为 null。</param>
public sealed record AdmissionReading(int State, TimeSpan SampleAge, TimeSpan? CapacityAge);

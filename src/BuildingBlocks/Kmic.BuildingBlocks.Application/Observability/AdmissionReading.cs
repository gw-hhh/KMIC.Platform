namespace Kmic.BuildingBlocks.Application.Observability;

/// <summary>
/// 消息写入的容量保护状态，量表读取用
/// </summary>
/// <param name="State">0 正常、1 软线、2 硬线</param>
/// <param name="SampleAge">判断结果距上次更新的时间</param>
/// <param name="CapacityAge">容量设置距上次更新的时间；没有设置容量时为空</param>
public sealed record AdmissionReading(int State, TimeSpan SampleAge, TimeSpan? CapacityAge);

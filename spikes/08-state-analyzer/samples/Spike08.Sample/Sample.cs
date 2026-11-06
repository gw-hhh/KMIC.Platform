using Kmic.BuildingBlocks.Domain;

namespace Spike08.Sample;

/// <summary>
/// 示例单据
/// </summary>
public sealed class Sample
{
    private static readonly StateMachine<Sample, SampleStatus, int> Machine =
        StateMachine.Define<Sample, SampleStatus, int>(s => s.Status, (s, v) => s.Status = v).Build();

    /// <summary>
    /// 状态
    /// </summary>
    public SampleStatus Status { get; private set; }

    /// <summary>
    /// 直接改状态，应报 KM0401
    /// </summary>
    public void Approve() => Status = SampleStatus.Approved;

    private sealed class Fixer
    {
        public static void Fix(Sample sample) => sample.Status = SampleStatus.Draft;
    }
}

/// <summary>
/// 示例状态
/// </summary>
public enum SampleStatus
{
    Draft,
    Approved,
}

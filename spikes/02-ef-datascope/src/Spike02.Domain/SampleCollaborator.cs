namespace Spike02.Domain;

/// <summary>
/// 示例聚合的协作人（关系可见，05-05 第 5.8 节）
/// </summary>
public sealed class SampleCollaborator(SampleId sampleId, UserId userId)
{
    public SampleId SampleId { get; private set; } = sampleId;

    public UserId UserId { get; private set; } = userId;
}

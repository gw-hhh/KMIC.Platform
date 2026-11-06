namespace Spike02.Domain;

/// <summary>
/// 示例聚合：同时有归属人、组织单元、协作人与私有标记，用来覆盖 05-05 第 5.4 节过滤器的全部分支
/// </summary>
public sealed class Sample : IHasOwner, IHasOrgUnit
{
    private readonly List<SampleCollaborator> _collaborators = [];

    public Sample(SampleId id, string title, UserId ownerId, OrgUnitId orgUnitId, bool isPrivate)
    {
        Id = id;
        Title = title;
        OwnerId = ownerId;
        OrgUnitId = orgUnitId;
        IsPrivate = isPrivate;
    }

    public SampleId Id { get; private set; }

    public string Title { get; private set; }

    public UserId OwnerId { get; private set; }

    public OrgUnitId OrgUnitId { get; private set; }

    public bool IsPrivate { get; private set; }

    public bool IsDeleted { get; private set; }

    public IReadOnlyList<SampleCollaborator> Collaborators => _collaborators;

    public void AddCollaborator(UserId userId) => _collaborators.Add(new SampleCollaborator(Id, userId));

    public void Delete() => IsDeleted = true;
}

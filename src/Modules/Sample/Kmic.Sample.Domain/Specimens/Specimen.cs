using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens.Events;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>
/// 示例标本
/// </summary>
/// <remarks>
/// 参考模块里的聚合，演示聚合的标准写法：状态机、子实体、领域事件、软删除、数据归属。
/// 聚合的名字不能和模块名相同：命名空间 Kmic.Sample 会遮住同名的类型
/// </remarks>
public sealed class Specimen : AggregateRoot<SpecimenId>, ISoftDeletable, IHasOwner, IHasOrgUnit
{
    private static readonly StateMachine<Specimen, SpecimenStatus, SpecimenAction> _machine =
        StateMachine.Define<Specimen, SpecimenStatus, SpecimenAction>(s => s.Status, (s, v) => s.Status = v)
            .Initial(SpecimenStatus.Draft)
            .OnInvalid(SampleErrors.InvalidState)
            .Permit(SpecimenStatus.Draft, SpecimenAction.Submit, SpecimenStatus.Submitted)
            .Permit(SpecimenStatus.Submitted, SpecimenAction.Close, SpecimenStatus.Closed)
            .Build();

    private readonly List<SpecimenItem> _items = [];

    private Specimen(SpecimenId id, string number, string title, UserId ownerId, OrgUnitId orgUnitId)
        : base(id)
    {
        Number = number;
        Title = title;
        OwnerId = ownerId;
        OrgUnitId = orgUnitId;
    }

    /// <summary>
    /// 编号，创建时由编号规则生成
    /// </summary>
    public string Number { get; private set; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; private set; }

    /// <summary>
    /// 状态
    /// </summary>
    public SpecimenStatus Status { get; private set; } = null!;

    /// <summary>
    /// 负责人，创建者
    /// </summary>
    public UserId OwnerId { get; private set; }

    /// <summary>
    /// 所属组织，数据范围按它判断
    /// </summary>
    public OrgUnitId OrgUnitId { get; private set; }

    /// <summary>
    /// 明细
    /// </summary>
    public IReadOnlyList<SpecimenItem> Items => _items;

    /// <summary>
    /// 创建草稿，状态的起点同时写入业务历史
    /// </summary>
    public static Specimen Create(string number, string title, UserId ownerId, OrgUnitId orgUnitId, string? reason = null)
    {
        var specimen = new Specimen(SpecimenId.New(), number, title, ownerId, orgUnitId);
        _machine.Start(specimen, reason);
        return specimen;
    }

    /// <summary>
    /// 添加一项明细，只有草稿可以
    /// </summary>
    public Result<SpecimenItem> AddItem(string name, int quantity)
    {
        if (Status != SpecimenStatus.Draft)
        {
            return SampleErrors.InvalidState;
        }

        var item = SpecimenItem.Create(name, quantity);
        _items.Add(item);
        return item;
    }

    /// <summary>
    /// 提交：至少要有一项明细，流转后发布“已提交”事件
    /// </summary>
    public Result Submit(string? reason = null)
    {
        if (_items.Count == 0)
        {
            return SampleErrors.NoItems;
        }

        var fired = _machine.Fire(this, SpecimenAction.Submit, reason);
        if (fired.IsSuccess)
        {
            Raise(new SpecimenSubmitted(Id));
        }

        return fired;
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public Result Close(string? reason = null) => _machine.Fire(this, SpecimenAction.Close, reason);

    /// <summary>
    /// 当前状态能否执行某个动作，给“可用动作”判断用
    /// </summary>
    public Result CanFire(SpecimenAction action) => _machine.CanFire(this, action);
}

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
    private readonly List<SpecimenAttachment> _attachments = [];

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
    /// 附件
    /// </summary>
    public IReadOnlyList<SpecimenAttachment> Attachments => _attachments;

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
    /// 能不能增删附件：关闭之后不能
    /// </summary>
    public Result CanEditAttachments() => Status == SpecimenStatus.Closed ? SampleErrors.InvalidState : Result.Success();

    /// <summary>
    /// 添加附件；同一个文件不能添加两次
    /// </summary>
    /// <param name="fileId">文件，已经由 IFileAttachments 取锁、检查、发出关联请求</param>
    /// <param name="fileName">关联时的文件名快照</param>
    /// <param name="size">关联时的字节数快照</param>
    /// <param name="fileType">关联时的文件类型编码快照</param>
    public Result<SpecimenAttachment> AddAttachment(StoredFileId fileId, string fileName, long size, string fileType)
    {
        var editable = CanEditAttachments();
        if (editable.IsFailure)
        {
            return editable.Error;
        }

        if (_attachments.Exists(a => a.FileId == fileId))
        {
            return SampleErrors.AttachmentExists;
        }

        var attachment = SpecimenAttachment.Create(fileId, fileName, size, fileType);
        _attachments.Add(attachment);
        return attachment;
    }

    /// <summary>
    /// 移除附件，返回被移除的那个，调用方据此解除文件的关联
    /// </summary>
    /// <param name="attachmentId">附件</param>
    public Result<SpecimenAttachment> RemoveAttachment(SpecimenAttachmentId attachmentId)
    {
        var editable = CanEditAttachments();
        if (editable.IsFailure)
        {
            return editable.Error;
        }

        var attachment = _attachments.Find(a => a.Id == attachmentId);
        if (attachment is null)
        {
            return SampleErrors.AttachmentNotFound;
        }

        _attachments.Remove(attachment);
        return attachment;
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
            Raise(new SpecimenSubmitted(Id, Number, Title, OwnerId));
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

using Kmic.BuildingBlocks.Domain.Entities;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.BuildingBlocks.Domain.StateMachines;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens.Events;

namespace Kmic.Sample.Domain.Specimens;

/// <summary>
/// 业务模块示例
/// </summary>
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
    /// 负责人
    /// </summary>
    public UserId OwnerId { get; private set; }

    /// <summary>
    /// 所属组织
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

    /// <summary>创建草稿并记录初始状态</summary>
    public static Specimen Create(string number, string title, UserId ownerId, OrgUnitId orgUnitId, string? reason = null)
    {
        var specimen = new Specimen(SpecimenId.New(), number, title, ownerId, orgUnitId);
        _machine.Start(specimen, reason);
        return specimen;
    }

    /// <summary>为草稿添加明细</summary>
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

    /// <summary>校验当前状态是否允许增删附件（已关闭后不允许）</summary>
    public Result CanEditAttachments() => Status == SpecimenStatus.Closed ? SampleErrors.InvalidState : Result.Success();

    /// <summary>添加附件；调用前须先经 IFileAttachments 校验文件可用性</summary>
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

    /// <summary>移除附件，成功时返回被移除的附件供调用方解除文件关联</summary>
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

    /// <summary>提交示例，至少须包含一项明细</summary>
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

    public Result Close(string? reason = null) => _machine.Fire(this, SpecimenAction.Close, reason);

    /// <summary>当前状态是否允许执行指定动作</summary>
    public Result CanFire(SpecimenAction action) => _machine.CanFire(this, action);
}

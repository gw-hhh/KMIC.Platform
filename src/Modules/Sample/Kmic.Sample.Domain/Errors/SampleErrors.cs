using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Sample.Domain.Errors;

/// <summary>
/// Sample 模块的错误目录
/// </summary>
public static class SampleErrors
{
    /// <summary>
    /// 示例不存在或当前用户无权查看
    /// </summary>
    public static readonly Error NotFound = Error.NotFound("Sample.NotFound", "示例不存在");

    /// <summary>
    /// 没有明细不能提交
    /// </summary>
    public static readonly Error NoItems = Error.BusinessRule("Sample.NoItems", "至少要有一项明细才能提交");

    /// <summary>
    /// 当前状态不能执行这个操作
    /// </summary>
    public static readonly Error InvalidState = Error.BusinessRule("Sample.InvalidState", "当前状态不能执行这个操作");

    /// <summary>
    /// 只有负责人可以操作
    /// </summary>
    public static readonly Error NotOwner = Error.Forbidden("Sample.NotOwner", "只有负责人可以操作");

    /// <summary>
    /// 编号重复
    /// </summary>
    public static readonly Error NumberDuplicated = Error.Conflict("Sample.NumberDuplicated", "示例编号已存在");

    /// <summary>
    /// 附件不存在
    /// </summary>
    public static readonly Error AttachmentNotFound = Error.NotFound("Sample.AttachmentNotFound", "附件不存在");

    /// <summary>
    /// 同一个文件已经是这个示例的附件
    /// </summary>
    public static readonly Error AttachmentExists = Error.Conflict("Sample.AttachmentExists", "该文件已添加为此示例的附件");

    /// <summary>
    /// 用户没有所属组织，无法确定数据归属
    /// </summary>
    public static readonly Error NoOrgUnit = Error.BusinessRule("Sample.NoOrgUnit", "当前用户没有所属组织，无法创建示例");
}

using Kmic.BuildingBlocks.Domain.Results;

namespace Kmic.Sample.Domain.Errors;

public static class SampleErrors
{
    /// <summary>示例不存在或不在当前用户的数据范围内</summary>
    public static readonly Error NotFound = Error.NotFound("Sample.NotFound", "示例不存在");

    public static readonly Error NoItems = Error.BusinessRule("Sample.NoItems", "至少要有一项明细才能提交");

    public static readonly Error InvalidState = Error.BusinessRule("Sample.InvalidState", "当前状态不允许此操作");

    public static readonly Error NotOwner = Error.Forbidden("Sample.NotOwner", "只有负责人可以操作");

    public static readonly Error NumberDuplicated = Error.Conflict("Sample.NumberDuplicated", "示例编号已存在");

    public static readonly Error AttachmentNotFound = Error.NotFound("Sample.AttachmentNotFound", "附件不存在");

    public static readonly Error AttachmentExists = Error.Conflict("Sample.AttachmentExists", "该文件已添加为此示例的附件");

    public static readonly Error NoOrgUnit = Error.BusinessRule("Sample.NoOrgUnit", "当前用户没有所属组织，无法创建示例");

    public static readonly Error CategoryNotFound = Error.NotFound("Sample.CategoryNotFound", "示例分类不存在");

    public static readonly Error CategoryNameExists = Error.Conflict("Sample.CategoryNameExists", "已有同名的示例分类");
}

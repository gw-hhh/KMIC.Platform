namespace Kmic.Files.Application.Administration;

/// <summary>业务对象对文件的引用</summary>
/// <param name="OwnerModule">引用文件的业务模块</param>
/// <param name="OwnerType">业务对象类型</param>
/// <param name="OwnerId">业务对象 ID</param>
/// <param name="AttachedAt">关联时间</param>
public sealed record FileReferenceItem(string OwnerModule, string OwnerType, Guid OwnerId, DateTimeOffset AttachedAt);

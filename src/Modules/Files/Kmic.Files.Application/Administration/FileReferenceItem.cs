namespace Kmic.Files.Application.Administration;

/// <summary>
/// 一个业务对象对文件的引用
/// </summary>
/// <param name="OwnerModule">引用它的业务模块</param>
/// <param name="OwnerType">业务对象的类型</param>
/// <param name="OwnerId">业务对象的 ID</param>
/// <param name="AttachedAt">关联的时间</param>
public sealed record FileReferenceItem(string OwnerModule, string OwnerType, Guid OwnerId, DateTimeOffset AttachedAt);

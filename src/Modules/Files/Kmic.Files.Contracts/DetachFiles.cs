using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Files.Contracts;

/// <summary>
/// 请求 Files 解除业务对象对一批文件的引用
/// </summary>
/// <remarks>
/// 与业务附件删除在同一事务中发送。全部引用解除后进入保留期，期间可重新关联，到期后由清理任务处理
/// </remarks>
/// <param name="FileIds">文件 ID</param>
/// <param name="OwnerModule">引用它们的业务模块</param>
/// <param name="OwnerType">业务对象的类型，如 Specimen</param>
/// <param name="OwnerId">业务对象的 ID</param>
[IntegrationMessage("Files.DetachFiles")]
[MessageSubjects(nameof(FileIds))]
public sealed record DetachFiles(IReadOnlyList<Guid> FileIds, string OwnerModule, string OwnerType, Guid OwnerId) : IIntegrationRequest;

using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Files.Contracts;

/// <summary>
/// 请求 Files 解除业务对象对一批文件的引用
/// </summary>
/// <remarks>
/// 请求型消息，和删除关联行在同一个业务事务里发。引用为零的文件进入“已解除关联”，保留期内可以重新关联，期满清理
/// </remarks>
/// <param name="FileIds">文件 ID</param>
/// <param name="OwnerModule">引用它们的业务模块</param>
/// <param name="OwnerType">业务对象的类型，如 Specimen</param>
/// <param name="OwnerId">业务对象的 ID</param>
[IntegrationMessage("Files.DetachFiles")]
[MessageSubjects(nameof(FileIds))]
public sealed record DetachFiles(IReadOnlyList<Guid> FileIds, string OwnerModule, string OwnerType, Guid OwnerId) : IIntegrationRequest;

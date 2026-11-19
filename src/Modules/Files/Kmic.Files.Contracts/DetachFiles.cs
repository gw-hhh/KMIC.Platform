using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Files.Contracts;

/// <summary>
/// 请求 Files 解除业务对象对一批文件的引用
/// </summary>
/// <param name="FileIds">文件 ID</param>
/// <param name="OwnerModule">引用它们的业务模块</param>
/// <param name="OwnerType">业务对象的类型</param>
/// <param name="OwnerId">业务对象的 ID</param>
[IntegrationMessage("Files.DetachFiles")]
[MessageSubjects(nameof(FileIds))]
public sealed record DetachFiles(IReadOnlyList<Guid> FileIds, string OwnerModule, string OwnerType, Guid OwnerId) : IIntegrationRequest;

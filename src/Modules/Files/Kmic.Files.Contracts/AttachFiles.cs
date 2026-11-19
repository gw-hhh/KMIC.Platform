using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Files.Contracts;

/// <summary>
/// 请求 Files 登记业务对象对一批文件的引用
/// </summary>
/// <remarks>
/// 只能经 <see cref="IFileAttachments" /> 发送
/// </remarks>
/// <param name="FileIds">文件 ID</param>
/// <param name="OwnerModule">引用它们的业务模块</param>
/// <param name="OwnerType">业务对象的类型</param>
/// <param name="OwnerId">业务对象的 ID</param>
[IntegrationMessage(AttachFiles.MessageName)]
[MessageSubjects(nameof(FileIds))]
public sealed record AttachFiles(IReadOnlyList<Guid> FileIds, string OwnerModule, string OwnerType, Guid OwnerId) : IIntegrationRequest
{
    /// <summary>
    /// 消息类型名，用于探查未完成的文件关联请求
    /// </summary>
    public const string MessageName = "Files.AttachFiles";
}

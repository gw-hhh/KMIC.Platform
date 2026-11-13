using Kmic.BuildingBlocks.Contracts.Messaging;

namespace Kmic.Files.Contracts;

/// <summary>
/// 请求 Files 登记业务对象对一批文件的引用
/// </summary>
/// <remarks>
/// 请求型消息，随业务事务写入 Outbox：业务回滚则不发，文件保持临时状态到期清理，不会留下孤儿引用。
/// 业务代码不直接发它，用 <see cref="IFileAttachments" />：它按互斥协议先取锁、检查，再发这条消息
/// </remarks>
/// <param name="FileIds">文件 ID</param>
/// <param name="OwnerModule">引用它们的业务模块</param>
/// <param name="OwnerType">业务对象的类型，如 Specimen</param>
/// <param name="OwnerId">业务对象的 ID</param>
[IntegrationMessage(AttachFiles.MessageName)]
[MessageSubjects(nameof(FileIds))]
public sealed record AttachFiles(IReadOnlyList<Guid> FileIds, string OwnerModule, string OwnerType, Guid OwnerId) : IIntegrationRequest
{
    /// <summary>
    /// 消息类型名：发件箱与收件箱里的 MessageType 就是它，清理文件前探查未完成的关联请求用
    /// </summary>
    public const string MessageName = "Files.AttachFiles";
}

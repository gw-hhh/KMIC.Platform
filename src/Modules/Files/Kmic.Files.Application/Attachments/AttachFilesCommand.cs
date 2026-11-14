using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.Attachments;

/// <summary>
/// 处理业务文件关联请求，登记引用并将文件转为已关联
/// </summary>
/// <remarks>
/// 订阅 Files.AttachFiles；可用或已解除关联的文件转为已关联，同一引用重复登记直接成功。
/// 业务事务已在锁内检查可关联性，文件不存在、删除中或已彻底删除均属异常，消息进入死信且不可放弃，须排除问题后重试
/// </remarks>
[SystemOnly]
[NotAudited("内部关联请求，状态变更记入文件历史")]
public sealed record AttachFilesCommand(AttachFiles Message) : ICommand;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.Attachments;

/// <summary>
/// 处理业务模块发来的关联请求：文件转“已关联”并登记引用
/// </summary>
/// <remarks>
/// 订阅 Files.AttachFiles。可用或已解除关联的文件转“已关联”；已是“已关联”且已有同一引用的幂等成功。
/// 文件已经在删除中、已彻底删除、不存在属于程序缺陷（业务事务已经在锁内检查过可关联性），进入死信，不可放弃，只能重试
/// </remarks>
/// <param name="Message">关联请求</param>
[SystemOnly]
[NotAudited("文件状态历史记录")]
public sealed record AttachFilesCommand(AttachFiles Message) : ICommand;

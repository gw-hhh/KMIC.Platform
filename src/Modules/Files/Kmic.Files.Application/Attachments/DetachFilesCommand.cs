using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.Attachments;

/// <summary>
/// 处理业务模块发来的解除关联请求：删除引用，引用为零的文件转“已解除关联”
/// </summary>
/// <remarks>
/// 订阅 Files.DetachFiles。引用不存在、文件已经不在了都幂等成功：解除关联的目的是文件不再被这个对象需要，到这里已经满足
/// </remarks>
/// <param name="Message">解除关联请求</param>
[SystemOnly]
[NotAudited("文件状态历史记录")]
public sealed record DetachFilesCommand(DetachFiles Message) : ICommand;

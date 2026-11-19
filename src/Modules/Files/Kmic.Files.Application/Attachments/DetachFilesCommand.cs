using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.Attachments;

/// <summary>
/// 解除业务文件引用，最后一个引用解除后进入保留期
/// </summary>
[SystemOnly]
[NotAudited("内部解除关联请求，状态变更记入文件历史")]
public sealed record DetachFilesCommand(DetachFiles Message) : ICommand;

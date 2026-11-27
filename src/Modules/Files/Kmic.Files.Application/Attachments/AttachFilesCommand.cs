using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.Attachments;

/// <summary>处理业务文件关联请求，登记引用并将文件转为已关联</summary>
[SystemOnly]
[NotAudited("内部关联请求，状态变更记入文件历史")]
public sealed record AttachFilesCommand(AttachFiles Message) : ICommand;

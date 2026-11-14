using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.Attachments;

/// <summary>
/// 登记业务模块生成的文件，初始状态为可用并设置到期时间
/// </summary>
/// <remarks>
/// 订阅 Files.RegisterGeneratedFile；同一文件 ID 重复登记直接成功
/// </remarks>
[SystemOnly]
[NotAudited("内部文件登记，初始状态记入文件历史")]
public sealed record RegisterGeneratedFileCommand(RegisterGeneratedFile Message) : ICommand;

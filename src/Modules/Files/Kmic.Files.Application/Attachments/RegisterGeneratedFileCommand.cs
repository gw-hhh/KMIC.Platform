using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;
using Kmic.Files.Contracts;

namespace Kmic.Files.Application.Attachments;

/// <summary>
/// 处理模块发来的“登记生成的文件”请求：按消息里的文件 ID 插入一行可用的文件，带到期时间
/// </summary>
/// <remarks>
/// 订阅 Files.RegisterGeneratedFile。同一个文件 ID 已经登记过的幂等成功：消息可能重复投递
/// </remarks>
/// <param name="Message">登记请求</param>
[SystemOnly]
[NotAudited("文件状态历史记录")]
public sealed record RegisterGeneratedFileCommand(RegisterGeneratedFile Message) : ICommand;

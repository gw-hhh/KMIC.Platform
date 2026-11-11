using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 登记缓存标签失效，可以让命令失败
/// </summary>
/// <param name="Tags">要失效的标签，同一个标签可以重复</param>
/// <param name="Fail">登记之后返回失败</param>
[AllowAuthenticated]
[NotAudited("测试命令")]
internal sealed record InvalidateCacheCommand(IReadOnlyList<string> Tags, bool Fail = false) : ICommand;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

/// <summary>
/// 取一个编号，可以在取号之后让命令失败
/// </summary>
/// <param name="Rule">规则名</param>
/// <param name="Partition">分组键</param>
/// <param name="FailAfterTaking">取号之后返回失败</param>
[AllowAuthenticated]
[NotAudited("测试命令")]
internal sealed record TakeNumberCommand(string Rule, string? Partition = null, bool FailAfterTaking = false) : ICommand<TakenNumber>;

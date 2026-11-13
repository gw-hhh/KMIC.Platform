using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

[AllowAuthenticated]
[NotAudited("测试命令")]
internal sealed record FindUnfinishedCommand(string MessageType, IReadOnlyList<Guid> Ids) : ICommand<LockedThings>;

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Infrastructure.Tests.Probe.Commands;

[AllowAuthenticated]
[NotAudited("测试命令")]
internal sealed record RenameTicketCommand(TicketId Id, long ExpectedVersion, string Title) : ICommand<VersionedResponse<TicketId>>;

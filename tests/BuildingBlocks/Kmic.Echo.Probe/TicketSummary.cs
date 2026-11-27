using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Echo.Probe;

public sealed class TicketSummary : IReadModel
{
    public Guid Id { get; init; }

    public long SourceVersion { get; init; }

    public ReferenceStatus Status { get; init; }

    public bool IsTerminal { get; init; }

    public ReadModelTerminalReason? TerminalReason { get; init; }

    public DateTimeOffset? TerminalAt { get; init; }

    public Guid? RebuildRunId { get; init; }

    public DateTimeOffset SyncedAt { get; init; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; init; } = "";
}

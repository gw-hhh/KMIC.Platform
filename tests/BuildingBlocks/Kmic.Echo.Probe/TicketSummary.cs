using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Contracts.References;

namespace Kmic.Echo.Probe;

/// <summary>
/// Probe 单据在 Echo 的读模型
/// </summary>
public sealed class TicketSummary : IReadModel
{
    /// <inheritdoc />
    public Guid Id { get; init; }

    /// <inheritdoc />
    public long SourceVersion { get; init; }

    /// <inheritdoc />
    public ReferenceStatus Status { get; init; }

    /// <inheritdoc />
    public bool IsTerminal { get; init; }

    /// <inheritdoc />
    public ReadModelTerminalReason? TerminalReason { get; init; }

    /// <inheritdoc />
    public DateTimeOffset? TerminalAt { get; init; }

    /// <inheritdoc />
    public Guid? RebuildRunId { get; init; }

    /// <inheritdoc />
    public DateTimeOffset SyncedAt { get; init; }

    /// <summary>
    /// 标题
    /// </summary>
    public string Title { get; init; } = "";
}

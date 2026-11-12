using Kmic.BuildingBlocks.Application.ReadModels;
using Kmic.BuildingBlocks.Contracts.Messaging;
using Kmic.BuildingBlocks.Contracts.References;
using Kmic.BuildingBlocks.Contracts.Snapshots;

namespace Kmic.BuildingBlocks.Application.Tests.Integration;

/// <summary>
/// 消息目录的样例，有的故意写错，只交给 IntegrationMessageCatalog.Build 校验
/// </summary>
internal static class IntegrationMessageSamples
{
    [IntegrationMessage("BuildingBlocks.ThingChanged")]
    public sealed record ThingChanged(Guid Id) : IIntegrationEvent;

    [IntegrationMessage("BuildingBlocks.ThingChanged", Version = 2)]
    [OrderingKey(nameof(Name))]
    public sealed record ThingChangedV2(Guid Id, string Name) : IIntegrationEvent;

    public sealed record Unnamed(Guid Id) : IIntegrationEvent;

    [IntegrationMessage("Sales.WrongPrefix")]
    public sealed record WrongPrefix(Guid Id) : IIntegrationEvent;

    [IntegrationMessage("BuildingBlocks.ThingChanged")]
    public sealed record SameNameAndVersion(Guid Id) : IIntegrationEvent;

    [IntegrationMessage("BuildingBlocks.ZeroVersion", Version = 0)]
    public sealed record ZeroVersion(Guid Id) : IIntegrationEvent;

    [IntegrationMessage("BuildingBlocks.ArchiveThing")]
    [OrderingKey(nameof(Id))]
    public sealed record ArchiveThing(Guid Id) : IIntegrationRequest;

    [IntegrationMessage("BuildingBlocks.EventWithSubjects")]
    [MessageSubjects(nameof(Ids))]
    public sealed record EventWithSubjects(IReadOnlyList<Guid> Ids) : IIntegrationEvent;

    [IntegrationMessage("Identity.SendCode", CarriesSecret = true, SecretLifetimeSeconds = 300)]
    public sealed record SendCodeLike(Guid UserId, string Code) : IIntegrationRequest;

    [IntegrationMessage("BuildingBlocks.SecretEvent", CarriesSecret = true, SecretLifetimeSeconds = 300)]
    public sealed record SecretEvent(Guid Id) : IIntegrationEvent;

    [IntegrationMessage("BuildingBlocks.SecretWithoutLifetime", CarriesSecret = true)]
    public sealed record SecretWithoutLifetime(Guid Id) : IIntegrationRequest;

    [IntegrationMessage("BuildingBlocks.SecretTooLong", CarriesSecret = true, SecretLifetimeSeconds = 601)]
    public sealed record SecretTooLong(Guid Id) : IIntegrationRequest;

    [IntegrationMessage("BuildingBlocks.LifetimeWithoutSecret", SecretLifetimeSeconds = 60)]
    public sealed record LifetimeWithoutSecret(Guid Id) : IIntegrationRequest;

    public sealed record AttachLike(Guid Owner, IReadOnlyList<Guid> FileIds);

    public interface IThingSnapshotSource : ISnapshotSource<ThingChanged>;

    public interface IOtherSnapshotSource : ISnapshotSource<ThingChangedV2>;

    public sealed record ThingRow(Guid Id) : IReadModel
    {
        public long SourceVersion { get; init; }

        public ReferenceStatus Status { get; init; }

        public bool IsTerminal { get; init; }

        public ReadModelTerminalReason? TerminalReason { get; init; }

        public DateTimeOffset? TerminalAt { get; init; }

        public Guid? RebuildRunId { get; init; }

        public DateTimeOffset SyncedAt { get; init; }
    }
}

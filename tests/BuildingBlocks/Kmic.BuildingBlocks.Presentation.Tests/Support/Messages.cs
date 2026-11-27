using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

// 框架测试消息：仅用于满足用例目录声明，实际执行由 FakeSender 预设应答替代
internal static class Messages
{
    public static readonly Type[] All =
    [
        typeof(GetThingQuery), typeof(CreateThingCommand), typeof(DeleteThingCommand), typeof(StartExportCommand),
        typeof(RenameThingCommand), typeof(EchoCommand), typeof(SystemCommand), typeof(ContractThingQuery),
        typeof(PublicQuery), typeof(MeQuery), typeof(ListThingsQuery),
    ];

    internal sealed record ThingDto(ThingId Id, string Name, ThingId? ParentId = null, DateTimeOffset? ClosedAt = null);

    [RequirePermission("Test.Thing.Read")]
    internal sealed record ListThingsQuery(PageRequest Page, SortRequest Sort) : IQuery<PagedResult<ThingDto>>;

    [RequirePermission("Test.Thing.Read")]
    internal sealed record GetThingQuery(ThingId Id) : IQuery<ThingDto>;

    [RequirePermission("Test.Thing.Write")]
    [Audited]
    [Idempotent]
    internal sealed record CreateThingCommand(string Name) : ICommand<CreatedResponse<ThingId>>;

    [RequirePermission("Test.Thing.Write")]
    [NotAudited("框架测试")]
    internal sealed record DeleteThingCommand(ThingId Id) : ICommand;

    [RequirePermission("Test.Thing.Export")]
    [NotAudited("框架测试")]
    internal sealed record StartExportCommand : ICommand<JobAccepted>;

    [RequirePermission("Test.Thing.Write")]
    [Audited]
    internal sealed record RenameThingCommand(ThingId Id, long ExpectedVersion, string Name) : ICommand<VersionedResponse<ThingId>>;

    [RequirePermission("Test.Thing.Write")]
    [NotAudited("框架测试")]
    internal sealed record EchoCommand(EchoPayload Payload) : ICommand<EchoPayload>;

    [SystemOnly]
    [NotAudited("框架测试")]
    internal sealed record SystemCommand : ICommand;

    [ContractQuery(ContractAccess.CallerScoped)]
    internal sealed record ContractThingQuery(ThingId Id) : IQuery<ThingDto>;

    [AllowAnonymous]
    internal sealed record PublicQuery : IQuery<string>;

    [AllowAuthenticated]
    internal sealed record MeQuery : IQuery<string>;
}

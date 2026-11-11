using Kmic.BuildingBlocks.Application.Jobs;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.UseCases;

namespace Kmic.BuildingBlocks.Presentation.Tests.Support;

/// <summary>
/// 框架测试用的消息：声明只为让用例目录接受它们，处理逻辑由 FakeSender 的预设应答替代
/// </summary>
internal static class Messages
{
    /// <summary>
    /// 用例目录默认包含的全部测试消息
    /// </summary>
    public static readonly Type[] All =
    [
        typeof(GetThingQuery), typeof(CreateThingCommand), typeof(DeleteThingCommand), typeof(StartExportCommand),
        typeof(RenameThingCommand), typeof(EchoCommand), typeof(SystemCommand), typeof(ContractThingQuery),
        typeof(PublicQuery), typeof(MeQuery),
    ];

    internal sealed record ThingDto(ThingId Id, string Name);

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

    [ContractQuery(ContractAccess.Reference)]
    internal sealed record ContractThingQuery(ThingId Id) : IQuery<ThingDto>;

    [AllowAnonymous]
    internal sealed record PublicQuery : IQuery<string>;

    [AllowAuthenticated]
    internal sealed record MeQuery : IQuery<string>;
}

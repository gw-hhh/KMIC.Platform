using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.Messages.Dead;

namespace Kmic.Operations.Presentation.Messages;

internal sealed class GetDeadMessageEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/messages/dead/{module}/{box}/{id}", HandleAsync)
            .Sends<GetDeadMessageQuery>()
            .WithSummary("查看一条死信的全部内容")
            .WithDescription("包括载荷与信封，也能看已经被放弃的消息。载荷原样显示：死信载荷按设计不含敏感字段，含密消息的载荷不进库。每次查看都会留下读取审计。");

    private static async Task<IResult> HandleAsync(
        [Description("所在模块，如 Sample")] string module,
        [Description("Outbox 或 Inbox")] string box,
        [Description("行 ID，来自死信列表")] Guid id,
        IQuerySender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new GetDeadMessageQuery(module, box, id), ct)).ToOk();
}

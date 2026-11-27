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
            .WithSummary("查看死信详情")
            .WithDescription("返回死信原始内容与信封信息（含已放弃消息，记录读取审计）");

    private static async Task<IResult> HandleAsync(
        [Description("所属模块，如 Sample")] string module,
        [Description("Outbox 或 Inbox")] string box,
        [Description("行 ID，来自死信列表")] Guid id,
        IQuerySender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new GetDeadMessageQuery(module, box, id), ct)).ToOk();
}

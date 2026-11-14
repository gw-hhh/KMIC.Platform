using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Notifications.Application.Captured.Get;
using Kmic.Notifications.Domain.Captured;

namespace Kmic.Notifications.Presentation.Captured;

internal sealed class GetCapturedEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/captured/{id}", HandleAsync)
            .Sends<GetCapturedQuery>()
            .WithSummary("查询捕获内容详情")
            .WithDescription("返回捕获内容及正文。未开启捕获或记录不存在时返回 404。内容可能含验证码，查看会记录读审计。");

    private static async Task<IResult> HandleAsync([Description("捕获内容 ID")] CapturedMessageId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetCapturedQuery(id), ct)).ToOk();
}

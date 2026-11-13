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
            .WithSummary("查询一条捕获的通知内容")
            .WithDescription("包含正文。没有开启捕获（生产环境）时返回 404。内容可能含验证码，查看会记读审计。");

    private static async Task<IResult> HandleAsync([Description("捕获内容 ID")] CapturedMessageId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetCapturedQuery(id), ct)).ToOk();
}

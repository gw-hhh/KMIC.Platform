using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Operations.Application.ClientErrors;

namespace Kmic.Operations.Presentation.ClientErrors;

internal sealed class ReportClientErrorsEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/client-errors", HandleAsync)
            .Sends<ReportClientErrorsCommand>()
            .WithSummary("批量上报前端错误")
            .WithDescription("仅写 Kmic.ClientErrors 日志不入库（含须改密用户可调），默认每批最多 20 条、每分钟限 30 次；超限返回 429 后直接丢弃，勿传请求体或查询参数");

    private static async Task<IResult> HandleAsync(ReportClientErrorsRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ReportClientErrorsCommand(request.Errors), ct)).ToNoContent();
}

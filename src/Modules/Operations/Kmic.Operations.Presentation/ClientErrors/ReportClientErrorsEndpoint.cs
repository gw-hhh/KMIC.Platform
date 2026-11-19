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
            .WithDescription("登录后可上报，包括必须修改密码的账号。仅写入 Kmic.ClientErrors 日志，不存入数据库，默认每批最多 20 条；请求按实例、按用户分别限流，默认每分钟最多 30 次。超限返回 429，前端应丢弃该批次，不重试。仅接收规定字段；上报内容不得包含请求体、表单值或 URL 查询参数。");

    private static async Task<IResult> HandleAsync(ReportClientErrorsRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ReportClientErrorsCommand(request.Errors), ct)).ToNoContent();
}

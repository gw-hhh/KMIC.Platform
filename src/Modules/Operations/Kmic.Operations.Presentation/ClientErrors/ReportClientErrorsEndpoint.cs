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
            .WithSummary("前端批量上报错误")
            .WithDescription("登录就能报（包括必须先改密码的账号），只写日志（分类 Kmic.ClientErrors），不落库，也不要求幂等键。一次最多 20 条，每个用户每分钟最多 30 次请求，超限返回 429，前端丢弃这批、不重试。字段是白名单，不接收请求体、表单值和地址里的查询参数。");

    private static async Task<IResult> HandleAsync(ReportClientErrorsRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ReportClientErrorsCommand(request.Errors), ct)).ToNoContent();
}

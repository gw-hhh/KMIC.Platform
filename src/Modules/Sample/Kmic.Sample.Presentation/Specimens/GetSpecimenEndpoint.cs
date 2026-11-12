using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Specimens.GetDetail;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Presentation.Specimens;

internal sealed class GetSpecimenEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/specimens/{id}", HandleAsync)
            .Sends<GetSpecimenDetailQuery>()
            .WithSummary("查询示例标本详情")
            .WithDescription("返回标本、明细、并发版本号和当前用户现在可以执行的动作。看不到的标本和不存在的一样，返回 404。");

    private static async Task<IResult> HandleAsync([Description("标本 ID")] SpecimenId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetSpecimenDetailQuery(id), ct)).ToOk();
}

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
            .WithSummary("查询示例详情")
            .WithDescription("返回示例信息、明细、附件、并发版本号和当前用户的可用操作。示例不存在或无权查看时均返回 404。");

    private static async Task<IResult> HandleAsync([Description("示例 ID")] SpecimenId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetSpecimenDetailQuery(id), ct)).ToOk();
}

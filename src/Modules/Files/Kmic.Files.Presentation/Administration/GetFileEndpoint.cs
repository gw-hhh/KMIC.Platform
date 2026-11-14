using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Files.Application.Administration;

namespace Kmic.Files.Presentation.Administration;

internal sealed class GetFileEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/{id}", HandleAsync)
            .Sends<GetFileQuery>()
            .WithSummary("查询文件详情")
            .WithDescription("返回文件元数据、状态、引用、状态历史及彻底删除记录，不提供存储键或下载地址。管理员下载也须通过业务对象授权。");

    private static async Task<IResult> HandleAsync([Description("文件 ID")] StoredFileId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetFileQuery(id), ct)).ToOk();
}

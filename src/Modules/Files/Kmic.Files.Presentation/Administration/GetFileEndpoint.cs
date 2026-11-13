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
            .WithDescription("元数据、状态、引用和彻底删除的记录。不返回存储键和下载地址：管理员也不经这里下载内容，下载走业务对象。");

    private static async Task<IResult> HandleAsync([Description("文件 ID")] StoredFileId id, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(new GetFileQuery(id), ct)).ToOk();
}

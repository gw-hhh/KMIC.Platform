using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Application.Querying;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Files.Application.Administration;

namespace Kmic.Files.Presentation.Administration;

internal sealed class ListFilesEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapGet("/", HandleAsync)
            .Sends<ListFilesQuery>()
            .Sortable(FileSort.Map)
            .WithSummary("查询文件列表")
            .WithDescription("文件的元数据、状态和引用，不含内容和存储键，分页。可以按状态、上传档案、上传人、上传时间和引用它的业务对象筛选。");

    private static async Task<IResult> HandleAsync([AsParameters] ListFilesRequest request, IQuerySender sender, CancellationToken ct) =>
        (await sender.SendAsync(
            new ListFilesQuery(
                request.Q,
                request.Status,
                request.Profile,
                request.UploadedBy,
                request.From,
                request.To,
                request.OwnerModule,
                request.OwnerType,
                request.OwnerId,
                new PageRequest(request.Page, request.PageSize),
                new SortRequest(request.Sort)),
            ct)).ToOk();
}

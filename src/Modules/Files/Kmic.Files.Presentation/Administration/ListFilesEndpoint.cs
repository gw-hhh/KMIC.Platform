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
            .WithDescription("分页查询文件元数据和状态，不含内容或存储键。支持按文件名、状态、上传配置、上传人或生成请求人、登记时间及引用文件的业务对象筛选。");

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

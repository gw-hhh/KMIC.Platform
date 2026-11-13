using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Files.Application.Administration;

namespace Kmic.Files.Presentation.Administration;

internal sealed class PurgeFileEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{id}/purge", HandleAsync)
            .Sends<PurgeFileCommand>()
            .WithSummary("彻底删除文件")
            .WithDescription("内容无法再读取，不可恢复；引用它的单据显示“文件已删除”。原因必填。历史版本要运维另行清除。已经彻底删除的再来一次算成功。业务正在关联这个文件时返回 503，稍后再试。");

    private static async Task<IResult> HandleAsync([Description("文件 ID")] StoredFileId id, PurgeFileRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PurgeFileCommand(id, request.Reason, request.ExpectedVersion), ct)).ToOk();
}

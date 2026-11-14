using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Files.Application.Administration;

namespace Kmic.Files.Presentation.Administration;

internal sealed class RetryDeletionEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{id}/retry-deletion", HandleAsync)
            .Sends<RetryDeletionCommand>()
            .WithSummary("重试删除文件")
            .WithDescription("删除尝试达到上限后停止自动重试。排除存储故障后，可重置删除队列中文件的尝试次数。文件不存在或不在删除队列中返回 404，成功返回 204。");

    private static async Task<IResult> HandleAsync([Description("文件 ID")] StoredFileId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RetryDeletionCommand(id), ct)).ToNoContent();
}

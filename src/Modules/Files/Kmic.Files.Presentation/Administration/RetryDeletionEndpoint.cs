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
            .WithDescription("排除存储故障后重置删除队列中的失败重试计数，文件不存在或不在队列中返回 404");

    private static async Task<IResult> HandleAsync([Description("文件 ID")] StoredFileId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RetryDeletionCommand(id), ct)).ToNoContent();
}

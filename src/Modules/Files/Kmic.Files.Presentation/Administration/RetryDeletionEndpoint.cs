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
            .WithSummary("重试删除")
            .WithDescription("删除失败太多次、不再自动重试的文件，排除存储的问题之后用它清零尝试次数。文件不在删除队列里返回 404。成功返回 204。");

    private static async Task<IResult> HandleAsync([Description("文件 ID")] StoredFileId id, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RetryDeletionCommand(id), ct)).ToNoContent();
}

using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Files.Application.Administration;

namespace Kmic.Files.Presentation.Administration;

internal sealed class RecordHistoryPurgeEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/{id}/history-purge", HandleAsync)
            .Sends<RecordHistoryPurgeCommand>()
            .WithSummary("登记文件历史版本清除记录")
            .WithDescription("运维清除对象存储中的文件历史版本后，登记执行记录（如工单号）。仅已彻底删除的文件可登记。");

    private static async Task<IResult> HandleAsync([Description("文件 ID")] StoredFileId id, RecordHistoryPurgeRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RecordHistoryPurgeCommand(id, request.Note, request.ExpectedVersion), ct)).ToOk();
}

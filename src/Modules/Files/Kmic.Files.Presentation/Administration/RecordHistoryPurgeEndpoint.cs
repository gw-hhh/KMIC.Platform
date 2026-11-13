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
            .WithSummary("登记已彻底删除历史版本")
            .WithDescription("运维在对象存储里清除了这个文件的历史版本之后，在这里留执行记录（如工单号）。只有已经彻底删除的文件可以登记。");

    private static async Task<IResult> HandleAsync([Description("文件 ID")] StoredFileId id, RecordHistoryPurgeRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new RecordHistoryPurgeCommand(id, request.Note, request.ExpectedVersion), ct)).ToOk();
}

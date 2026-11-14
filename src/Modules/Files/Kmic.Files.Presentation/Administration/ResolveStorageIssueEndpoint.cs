using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Files.Application.Administration;
using Kmic.Files.Domain.Files;

namespace Kmic.Files.Presentation.Administration;

internal sealed class ResolveStorageIssueEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/storage-issues/{id}/resolve", HandleAsync)
            .Sends<ResolveStorageIssueCommand>()
            .WithSummary("标记存储问题已解决")
            .WithDescription("登记存储问题的处理结果，不执行对象还原。处理说明必填。版本校验通过后，已解决的问题仍返回成功，并保留原处理人、时间和说明。");

    private static async Task<IResult> HandleAsync([Description("存储问题 ID")] StorageIssueId id, ResolveStorageIssueRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResolveStorageIssueCommand(id, request.Note, request.ExpectedVersion), ct)).ToOk();
}

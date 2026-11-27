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
            .WithDescription("登记存储问题处理结果（不执行还原），说明必填；已解决的问题通过版本校验后幂等返回原记录");

    private static async Task<IResult> HandleAsync([Description("存储问题 ID")] StorageIssueId id, ResolveStorageIssueRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new ResolveStorageIssueCommand(id, request.Note, request.ExpectedVersion), ct)).ToOk();
}

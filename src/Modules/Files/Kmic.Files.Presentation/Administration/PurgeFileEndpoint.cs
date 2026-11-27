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
            .WithDescription("不可逆地彻底删除文件（引用方显示“文件已删除”，历史版本须由运维另行清除）；已删除或删除中重入幂等成功，并发关联时返回 503");

    private static async Task<IResult> HandleAsync([Description("文件 ID")] StoredFileId id, PurgeFileRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PurgeFileCommand(id, request.Reason, request.ExpectedVersion), ct)).ToOk();
}

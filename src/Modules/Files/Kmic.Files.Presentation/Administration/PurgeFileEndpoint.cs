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
            .WithDescription("彻底删除后无法读取内容，操作不可恢复，引用方显示“文件已删除”。删除原因必填，历史版本须由运维另行清除。版本校验通过后，文件已彻底删除或正在删除时重复操作直接成功。存在并发关联或未完成的关联请求时返回 503，可稍后重试。");

    private static async Task<IResult> HandleAsync([Description("文件 ID")] StoredFileId id, PurgeFileRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new PurgeFileCommand(id, request.Reason, request.ExpectedVersion), ct)).ToOk();
}

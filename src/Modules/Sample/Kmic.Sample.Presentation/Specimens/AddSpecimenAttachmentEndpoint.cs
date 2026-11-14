using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Specimens.Attachments.Add;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Presentation.Specimens;

internal sealed class AddSpecimenAttachmentEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapPost("/specimens/{id}/attachments", HandleAsync)
            .Sends<AddSpecimenAttachmentCommand>()
            .WithSummary("为示例添加附件")
            .WithDescription("先通过上传接口上传文件（上传配置 Sample.SpecimenAttachment），再使用返回的文件 ID 添加附件。只有负责人可以添加，示例关闭后不能添加。支持本人上传且可用的临时文件，以及已解除关联、尚未进入清理流程的文件。");

    private static async Task<IResult> HandleAsync([Description("示例 ID")] SpecimenId id, AddSpecimenAttachmentRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AddSpecimenAttachmentCommand(id, StoredFileId.From(request.FileId), request.ExpectedVersion), ct)).ToOk();
}

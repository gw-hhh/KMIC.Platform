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
            .WithDescription("仅负责人可为未关闭示例添加附件，支持本人上传或已解除关联且未清理的文件（上传配置 Sample.SpecimenAttachment）");

    private static async Task<IResult> HandleAsync([Description("示例 ID")] SpecimenId id, AddSpecimenAttachmentRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AddSpecimenAttachmentCommand(id, StoredFileId.From(request.FileId), request.ExpectedVersion), ct)).ToOk();
}

using System.ComponentModel;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Presentation.Endpoints;
using Kmic.BuildingBlocks.Presentation.Http;
using Kmic.Sample.Application.Specimens.Attachments.Remove;
using Kmic.Sample.Domain.Specimens;
using Microsoft.AspNetCore.Mvc;

namespace Kmic.Sample.Presentation.Specimens;

internal sealed class RemoveSpecimenAttachmentEndpoint : IEndpoint
{
    public void Map(IEndpointRouteBuilder group) =>
        group.MapDelete("/specimens/{id}/attachments/{attachmentId}", HandleAsync)
            .Sends<RemoveSpecimenAttachmentCommand>()
            .WithSummary("移除示例标本的附件")
            .WithDescription("只有负责人可以移除，已关闭的标本不能移除。文件解除关联后保留一段时间，期间可以重新关联。成功返回 204。");

    private static async Task<IResult> HandleAsync(
        [Description("标本 ID")] SpecimenId id,
        [Description("附件 ID")] SpecimenAttachmentId attachmentId,
        [FromBody] RemoveSpecimenAttachmentRequest request,
        ICommandSender sender,
        CancellationToken ct) =>
        (await sender.SendAsync(new RemoveSpecimenAttachmentCommand(id, attachmentId, request.ExpectedVersion), ct)).ToNoContent();
}

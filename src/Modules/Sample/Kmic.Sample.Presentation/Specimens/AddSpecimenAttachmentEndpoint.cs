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
            .WithSummary("添加示例标本的附件")
            .WithDescription("文件要先经上传端点上传（上传档案 Sample.SpecimenAttachment），再用得到的文件 ID 添加。只有负责人可以添加，已关闭的标本不能添加；文件必须是本人上传、没有过期的临时文件。");

    private static async Task<IResult> HandleAsync([Description("标本 ID")] SpecimenId id, AddSpecimenAttachmentRequest request, ICommandSender sender, CancellationToken ct) =>
        (await sender.SendAsync(new AddSpecimenAttachmentCommand(id, StoredFileId.From(request.FileId), request.ExpectedVersion), ct)).ToOk();
}

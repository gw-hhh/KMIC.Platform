using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Sample.Contracts;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.Remove;

/// <summary>
/// 处理 <see cref="RemoveSpecimenAttachmentCommand"/>
/// </summary>
public sealed class RemoveSpecimenAttachmentCommandHandler(
    ISpecimenRepository specimens,
    IResourceAuthorizer<Specimen, SpecimenAction> authorizer,
    IFileAttachments files)
    : ICommandHandler<RemoveSpecimenAttachmentCommand, VersionedResponse<SpecimenId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<SpecimenId>>> Handle(RemoveSpecimenAttachmentCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await specimens.GetForUpdateAsync(command.Id, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var specimen = loaded.Value;
        var allowed = authorizer.Check(specimen, SpecimenAction.Edit);
        if (allowed.IsFailure)
        {
            return allowed.Error;
        }

        var removed = specimen.RemoveAttachment(command.AttachmentId);
        if (removed.IsFailure)
        {
            return removed.Error;
        }

        // 删除附件行和解除关联在同一个事务里：事务回滚则不发解除关联的请求
        await files.DetachAsync(SampleModuleInfo.Name, nameof(Specimen), specimen.Id.Value, [removed.Value.FileId.Value], cancellationToken);
        return new VersionedResponse<SpecimenId>(specimen.Id);
    }
}

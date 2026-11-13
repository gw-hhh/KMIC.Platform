using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Identifiers;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Files.Contracts;
using Kmic.Sample.Contracts;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Attachments.Add;

/// <summary>
/// 处理 <see cref="AddSpecimenAttachmentCommand"/>
/// </summary>
public sealed class AddSpecimenAttachmentCommandHandler(
    ISpecimenRepository specimens,
    IResourceAuthorizer<Specimen, SpecimenAction> authorizer,
    IFileAttachments files)
    : ICommandHandler<AddSpecimenAttachmentCommand, VersionedResponse<SpecimenId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<SpecimenId>>> Handle(AddSpecimenAttachmentCommand command, CancellationToken cancellationToken)
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

        // 业务上不能改的先拒绝，不必去取文件的锁
        var editable = specimen.CanEditAttachments();
        if (editable.IsFailure)
        {
            return editable.Error;
        }

        var attached = await files.AttachAsync(SampleModuleInfo.Name, nameof(Specimen), specimen.Id.Value, [command.FileId.Value], cancellationToken);
        if (attached.IsFailure)
        {
            return attached.Error;
        }

        var file = attached.Value[0];
        var added = specimen.AddAttachment(StoredFileId.From(file.FileId), file.Name, file.Size, file.FileType);
        return added.IsFailure ? added.Error : new VersionedResponse<SpecimenId>(specimen.Id);
    }
}

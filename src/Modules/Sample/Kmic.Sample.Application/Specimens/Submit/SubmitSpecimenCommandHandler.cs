using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Submit;

/// <summary>
/// 提交示例
/// </summary>
public sealed class SubmitSpecimenCommandHandler(ISpecimenRepository specimens, IResourceAuthorizer<Specimen, SpecimenAction> authorizer)
    : ICommandHandler<SubmitSpecimenCommand, VersionedResponse<SpecimenId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<SpecimenId>>> Handle(SubmitSpecimenCommand command, CancellationToken cancellationToken)
    {
        var loaded = await specimens.GetForUpdateAsync(command.Id, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var specimen = loaded.Value;
        var allowed = authorizer.Check(specimen, SpecimenAction.Submit);
        if (allowed.IsFailure)
        {
            return allowed.Error;
        }

        var submitted = specimen.Submit();
        return submitted.IsFailure ? submitted.Error : new VersionedResponse<SpecimenId>(specimen.Id);
    }
}

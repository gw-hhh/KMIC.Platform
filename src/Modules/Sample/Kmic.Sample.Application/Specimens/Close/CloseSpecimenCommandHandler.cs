using Kmic.BuildingBlocks.Application.Authorization;
using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Close;

/// <summary>
/// 处理 <see cref="CloseSpecimenCommand"/>
/// </summary>
public sealed class CloseSpecimenCommandHandler(ISpecimenRepository specimens, IResourceAuthorizer<Specimen, SpecimenAction> authorizer)
    : ICommandHandler<CloseSpecimenCommand, VersionedResponse<SpecimenId>>
{
    /// <inheritdoc />
    public async ValueTask<Result<VersionedResponse<SpecimenId>>> Handle(CloseSpecimenCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await specimens.GetForUpdateAsync(command.Id, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        var specimen = loaded.Value;
        var allowed = authorizer.Check(specimen, SpecimenAction.Close);
        if (allowed.IsFailure)
        {
            return allowed.Error;
        }

        var closed = specimen.Close();
        return closed.IsFailure ? closed.Error : new VersionedResponse<SpecimenId>(specimen.Id);
    }
}

using Kmic.BuildingBlocks.Application.Messaging;
using Kmic.BuildingBlocks.Domain.Results;
using Kmic.Sample.Domain.Errors;
using Kmic.Sample.Domain.Specimens;

namespace Kmic.Sample.Application.Specimens.Delete;

/// <summary>
/// 处理 <see cref="DeleteSpecimenCommand"/>
/// </summary>
public sealed class DeleteSpecimenCommandHandler(ISpecimenRepository specimens) : ICommandHandler<DeleteSpecimenCommand>
{
    /// <inheritdoc />
    public async ValueTask<Result> Handle(DeleteSpecimenCommand command, CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var loaded = await specimens.GetForUpdateAsync(command.Id, command.ExpectedVersion, cancellationToken);
        if (loaded.IsFailure)
        {
            return loaded.Error;
        }

        if (loaded.Value.Status != SpecimenStatus.Draft)
        {
            return SampleErrors.InvalidState;
        }

        specimens.Remove(loaded.Value);
        return Result.Success();
    }
}
